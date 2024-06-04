// Ignore Spelling: Api app

using HttpServerSim.Contracts;
using HttpServerSim.Models;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text;

namespace HttpServerSim;

public static class HttpSimRuleResolverExtension
{
    public static IApplicationBuilder UseHttpSimRuleResolver(this IApplicationBuilder app, IHttpSimRuleResolver httpSimRuleResolver, ILogger logger)
    {
        return app.Use(CreateMiddlewareHandleRequest(httpSimRuleResolver, logger));
    }

    private static Func<HttpContext, RequestDelegate, Task> CreateMiddlewareHandleRequest(IHttpSimRuleResolver httpSimRuleResolver, ILogger logger)
    {
        return async (context, next) =>
        {
            if (httpSimRuleResolver == null)
            {
                await next(context);
                return;
            }

            var httpSimRequest = await MapRequestAsync(context.Request);
            var httpSimRule = httpSimRuleResolver.Resolve(httpSimRequest);

            if (httpSimRule == null)
            {
                logger.LogWarning("Rule matching the request not found.");
                await RuleNotFoundAsync(context);
                return;
            }

            logger.LogDebug($"Rule matching the request found. {httpSimRule.Name}");
            httpSimRule.Callback?.Invoke(httpSimRequest);

            if (httpSimRule.Response == null && httpSimRule.CreateResponseCallback == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }

            var response = httpSimRule.Response ?? httpSimRule.CreateResponseCallback!(httpSimRequest);
            SetHttpResponse(context, response, logger);
        };
    }

    private static void SetHttpResponse(HttpContext context, HttpSimResponse httpSimResponse, ILogger logger)
    {
        context.Response.StatusCode = httpSimResponse.StatusCode;
        SetHttpResponseHeader(context, httpSimResponse);
        SetHttpResponseContent(context, httpSimResponse, logger);
    }

    private static void SetHttpResponseContent(HttpContext context, HttpSimResponse httpSimResponse, ILogger logger)
    {
        if (httpSimResponse.ContentValue == null)
        {
            return;
        }

        context.Response.ContentType = httpSimResponse.ContentType;
        try
        {
            byte[] buffer = httpSimResponse.ContentValueType switch
            {
                ContentValueType.Text => Encoding.ASCII.GetBytes(httpSimResponse.ContentValue),
                ContentValueType.File => File.ReadAllBytes(httpSimResponse.ContentValue),
                _ => throw new InvalidOperationException($"Unexpected {httpSimResponse.ContentValueType}: '{httpSimResponse.ContentValueType}'."),
            };
            context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting response content");
            throw;
        }
    }

    private static void SetHttpResponseHeader(HttpContext context, HttpSimResponse httpSimResponse)
    {
        if (httpSimResponse.Headers == null)
        {
            return;
        }

        foreach (var soureHeader in httpSimResponse.Headers)
        {
            var values = new StringValues(soureHeader.Value);
            context.Response.Headers[soureHeader.Key] = values;
        }
    }

    private static async Task RuleNotFoundAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        await context.Response.WriteAsync("Rule matching request not found");
    }

    private static async Task<HttpSimRequest> MapRequestAsync(HttpRequest httpRequest)
    {
        var httpSimRequest = new HttpSimRequest(httpRequest.Method, httpRequest.Path);
        var readResult = await httpRequest.BodyReader.ReadAsync();
        if (readResult.Buffer.Length > 0)
        {
            httpSimRequest.ContentValue = Encoding.ASCII.GetString(readResult.Buffer);
        }

        return httpSimRequest;
    }
}

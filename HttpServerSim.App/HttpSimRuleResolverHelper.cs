﻿// Ignore Spelling: Api app Middleware

using HttpServerSim.Client;
using HttpServerSim.Contracts;
using HttpServerSim.Models;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text;

namespace HttpServerSim;

internal static class HttpSimRuleResolverHelper
{
    /// <summary>
    /// Creates middleware for finding a rule that matches a request.
    /// </summary>
    /// <param name="httpSimRuleResolver"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static Func<HttpContext, RequestDelegate, Task> CreateMiddlewareHandleRequest(IHttpSimRuleResolver httpSimRuleResolver, ILogger logger)
    {
        return async (context, next) =>
        {
            var isRequestForControlEndpoint = context.Request.Path.ToString().Contains(Routes.CONTROL_ENDPOINT, StringComparison.InvariantCultureIgnoreCase);
            if (httpSimRuleResolver == null || isRequestForControlEndpoint)
            {
                await next(context);
                return;
            }

            var httpSimRequest = await MapRequestAsync(context.Request);
            var httpSimRule = httpSimRuleResolver.Resolve(httpSimRequest);

            if (httpSimRule == null)
            {
                logger.LogWarning("Rule matching request not found.");
                await RuleNotFoundAsync(context);
                return;
            }

            logger.LogDebug($"Rule matching request found. '{httpSimRule.Name}'");
            httpSimRule.AddRequest(httpSimRequest);
            if (httpSimRule.Response == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            
            SetHttpResponse(context, httpSimRule.Response, logger);
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

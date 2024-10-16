// Ignore Spelling: Api app Middleware

using HttpServerSim.App.Contracts;
using HttpServerSim.App.Models;
using HttpServerSim.Client;
using HttpServerSim.Client.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace HttpServerSim.App.Middleware;

internal static class HttpSimRuleResolver
{
    public static Func<HttpContext, RequestDelegate, Task> CreateMiddlewareRequestResponseLogger(IRequestResponseLogger requestResponseLogger)
    {
        return async (context, next) =>
        {
            var id = Guid.NewGuid().ToString();
            await requestResponseLogger.LogRequestAsync(context, id);

            var originalResponseBody = context.Response.Body;
            try
            {
                using var ms = new MemoryStream();
                context.Response.Body = ms;

                await next(context);

                ms.Position = 0;
                await requestResponseLogger.LogResponseAsync(context, id);
                ms.Position = 0;
                await ms.CopyToAsync(originalResponseBody);
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        };
    }

    /// <summary>
    /// Creates middleware for finding a rule that matches a request.
    /// </summary>
    public static Func<HttpContext, RequestDelegate, Task> CreateMiddlewareHandleRequest(IHttpSimRuleResolver httpSimRuleResolver, 
        ILogger logger, 
        string responseFilesFolder,
        DefaultResponse defaultResponse)
    {
        ArgumentNullException.ThrowIfNull(defaultResponse);

        return async (context, next) =>
        {
            var isRequestForControlEndpoint = context.Request.Path.ToString().Contains(Routes.CONTROL_ENDPOINT, StringComparison.InvariantCultureIgnoreCase);
            if (httpSimRuleResolver == null || isRequestForControlEndpoint)
            {
                await next(context);
                return;
            }

            var httpSimRequest = await MapRequestAsync(context.Request);
            var ruleManager = httpSimRuleResolver.Resolve(httpSimRequest);

            if (ruleManager == null)
            {
                logger.LogDebug("Rule matching request not found. Using the default response");
                await SetHttpResponseAsync(context, defaultResponse.Response, logger, responseFilesFolder);
                await IntroduceDelayAsync(logger, defaultResponse.Delay);
                return;
            }

            var rule = ruleManager.Rule;
            logger.LogDebug($"Rule matching request found. '{rule.Name}'");
            ruleManager.AddRequest(httpSimRequest);

            if (rule.Response == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                await SetHttpResponseAsync(context, rule.Response, logger, responseFilesFolder);
            }

            await IntroduceDelayAsync(logger, rule.Delay);
        };
    }

    private static async Task IntroduceDelayAsync(ILogger logger, DelayRange? delay)
    {
        if (delay is null)
        {
            return;
        }

        var actualDelay = delay.Max.HasValue ? new Random().Next(delay.Min, delay.Max.Value) : delay.Min;
        
        logger.LogDebug($"Adding a delay of: {actualDelay} Milliseconds");
        await Task.Delay(actualDelay);
    }

    private static async Task SetHttpResponseAsync(HttpContext context, HttpSimResponse httpSimResponse, ILogger logger, string responseFilesFolder)
    {
        try
        {
            context.Response.StatusCode = httpSimResponse.StatusCode;
            SetHttpResponseHeader(context, httpSimResponse);
            await SetHttpResponseContentAsync(context, httpSimResponse, logger, responseFilesFolder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting response");
            logger.LogError(ex.ToString());
            var buffer = Encoding.ASCII.GetBytes("Simulator Error - Error setting a response");
            await context.Response.Body.WriteAsync(buffer);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    private static byte[] GetBytesFromFile(string path, ILogger logger)
    {
        logger.LogDebug($"Getting content from file: '{path}'");
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }

        logger.LogWarning($"File '{path}' not found");
        return [];
    }

    private static async Task SetHttpResponseContentAsync(HttpContext context, HttpSimResponse httpSimResponse, ILogger logger, string responseFilesFolder)
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
                ContentValueType.File => GetBytesFromFile(Path.Combine(responseFilesFolder, httpSimResponse.ContentValue), logger),
                _ => throw new InvalidOperationException($"Unexpected {httpSimResponse.ContentValueType}: '{httpSimResponse.ContentValueType}'."),
            };

            if (httpSimResponse.Encoding == HttpSimResponseEncoding.GZip)
            {
                var syncIoFeature = context.Features.Get<IHttpBodyControlFeature>();
                if (syncIoFeature != null)
                {
                    syncIoFeature.AllowSynchronousIO = true;
                }

                using var compressor = new GZipStream(context.Response.Body, CompressionLevel.SmallestSize, leaveOpen: true);
                await compressor.WriteAsync(buffer);
            }
            else
            {
                await context.Response.Body.WriteAsync(buffer);
            }
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

        foreach (var sourceHeader in httpSimResponse.Headers)
        {
            var values = new StringValues(sourceHeader.Value);
            context.Response.Headers[sourceHeader.Key] = values;
        }
    }

    private static async Task<HttpSimRequest> MapRequestAsync(HttpRequest request)
    {
        var httpSimRequest = new HttpSimRequest(request.Method, request.Path);

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        if (body != string.Empty)
        {
            httpSimRequest.ContentValue = body;
        }

        return httpSimRequest;
    }
}

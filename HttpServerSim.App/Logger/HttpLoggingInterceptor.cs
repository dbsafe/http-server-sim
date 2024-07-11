using Microsoft.AspNetCore.HttpLogging;

namespace HttpServerSim.App.Logger;

public sealed class HttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        AddHeaders(logContext.HttpContext.Request.Headers, logContext);
        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        AddHeaders(logContext.HttpContext.Response.Headers, logContext);
        return default;
    }

    private static void AddHeaders(IHeaderDictionary headers, HttpLoggingInterceptorContext logContext)
    {
        foreach (var header in headers)
        {
            logContext.AddParameter(header.Key, header.Value);
        }
    }
}

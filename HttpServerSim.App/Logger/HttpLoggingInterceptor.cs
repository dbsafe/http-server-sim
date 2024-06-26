using Microsoft.AspNetCore.HttpLogging;

namespace HttpServerSim.App.Logger;

public sealed class HttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        AddHeaders(logContext);
        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        AddHeaders(logContext);
        return default;
    }

    private static void AddHeaders(HttpLoggingInterceptorContext logContext)
    {
        foreach (var header in logContext.HttpContext.Response.Headers)
        {
            logContext.AddParameter(header.Key, header.Value);
        }
    }
}

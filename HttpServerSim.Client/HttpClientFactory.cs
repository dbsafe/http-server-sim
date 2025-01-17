using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerSim.Client;

public static class HttpClientFactory
{
    public static HttpClient CreateHttpClient(string name) => new(new LoggingHandler(name, new HttpClientHandler()));
    public static HttpClient CreateHttpClient(string name, DecompressionMethods decompressionMethods)
    {
        var httpClientHandler = new HttpClientHandler { AutomaticDecompression = decompressionMethods };
        return new(new LoggingHandler(name, httpClientHandler));
    }
}

public class LoggingHandler(string name, HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private readonly string name = name;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        var sb = new StringBuilder();
        sb.AppendLine($"Request: [CorrelationId: {correlationId}]");
        var requestBeforeSent = request.ToString();
        sb.AppendLine(requestBeforeSent);
        await LogContentAsync(request.Content, sb);
        Console.WriteLine($"{name}{Environment.NewLine}{sb}");
        sb.Clear();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        var requestAfterSent = request.ToString();
        if (requestAfterSent != requestBeforeSent)
        {
            sb.AppendLine($"Request: [CorrelationId: {correlationId}]. Updated during sending.");
            sb.AppendLine(requestAfterSent);
            Console.WriteLine($"{name}{Environment.NewLine}{sb}");
            sb.Clear();
        }

        sb.AppendLine($"Response: [CorrelationId: {correlationId}]");
        sb.AppendLine(response.ToString());
        await LogContentAsync(response.Content, sb);
        Console.WriteLine($"{name}{Environment.NewLine}{sb}");

        return response;
    }

    private static async Task LogContentAsync(HttpContent? content, StringBuilder sb)
    {
        if (content != null)
        {
            sb.Append("Body:");
            sb.AppendLine(await content.ReadAsStringAsync());
        }
    }
}
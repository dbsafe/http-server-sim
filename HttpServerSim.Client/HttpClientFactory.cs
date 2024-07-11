using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HttpServerSim.Client;

public static class HttpClientFactory
{
    public static HttpClient CreateHttpClient(string name) => new(new LoggingHandler(name, new HttpClientHandler()));
}

public class LoggingHandler(string name, HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private readonly string name = name;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Request:");
        sb.AppendLine(request.ToString());
        await LogContentAsync(request.Content, sb);
        Console.WriteLine($"{name}{Environment.NewLine}{sb}");
        sb.Clear();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        sb.AppendLine("Response:");
        sb.AppendLine(response.ToString());
        await LogContentAsync(response.Content, sb);
        Console.WriteLine($"{name}{Environment.NewLine}{sb}");

        return response;
    }

    private async Task LogContentAsync(HttpContent? content, StringBuilder sb)
    {
        if (content != null)
        {
            sb.Append("Body:");
            sb.AppendLine(await content.ReadAsStringAsync());
        }
    }
}
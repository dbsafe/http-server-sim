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
        if (request.Content != null)
        {
            sb.Append("Body:");
            sb.AppendLine(await request.Content.ReadAsStringAsync());
        }

        Console.WriteLine($"{name}{Environment.NewLine}{sb}");
        sb.Clear();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        sb.AppendLine("Response:");
        sb.AppendLine(response.ToString());
        if (response.Content != null)
        {
            sb.Append("Body:");
            sb.AppendLine(await response.Content.ReadAsStringAsync());
        }

        Console.WriteLine($"{name}{Environment.NewLine}{sb}");

        return response;
    }
}
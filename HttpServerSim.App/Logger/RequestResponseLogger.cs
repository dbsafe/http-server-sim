// Ignore Spelling: App

using HttpServerSim.App.Contracts;
using System.Text;

namespace HttpServerSim;

public interface IConsoleRequestResponseLoggerConfig
{
    int RequestBodyLogLimit { get; }
    int ResponseBodyLogLimit { get; }
}

public class ConsoleRequestResponseLogger(ILogger appLogger, IConsoleRequestResponseLoggerConfig config) : IRequestResponseLogger
{
    public ILogger AppLogger { get; } = appLogger;

    public async Task LogRequestAsync(HttpContext context)
    {
        try
        {
            var request = context.Request ?? throw new InvalidOperationException("Request cannot be null");

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{ConsoleColors.Underline}Request:{ConsoleColors.NoUnderline}");
            sb.AppendLine($"{request.Protocol} - {request.Method} - {request.Scheme}://{request.Host}{request.Path}{request.QueryString}");

            AddHeaders(sb, request.Headers);

            await AddContentAsync(sb, request, config.RequestBodyLogLimit);
            sb.AppendLine("End of Request");

            Console.WriteLine($"{ConsoleColors.RequestColor}{sb}{ConsoleColors.NormalColor}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error while logging a request to the Console.{Environment.NewLine}{ex}");
        }
    }

    private static void AddHeaders(StringBuilder sb, IHeaderDictionary headers)
    {
        sb.AppendLine("Headers:");
        var hasHeaders = false;
        foreach (var header in headers)
        {
            sb.AppendLine($"  {header.Key}: {header.Value}");
            hasHeaders = true;
        }

        if (!hasHeaders)
        {
            sb.AppendLine("[Not present]");
        }
    }

    private static async Task AddContentAsync(StringBuilder sb, HttpRequest request, int logBodyLimit)
    {
        sb.AppendLine("Body:");
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        if (body == string.Empty)
        {
            sb.AppendLine("[Not present]");
            return;
        }

        var size = Math.Min(body.Length, logBodyLimit);
        sb.AppendLine(body.Substring(0, size));
        var isTruncated = size < body.Length;
        if (isTruncated)
        {
            sb.AppendLine($"[Body truncated. Read {logBodyLimit} characters]");
        }
    }

    public async Task LogResponseAsync(HttpContext context)
    {
        try
        {
            var response = context.Response ?? throw new InvalidOperationException("Response cannot be null");

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{ConsoleColors.Underline}Response:{ConsoleColors.NoUnderline}");
            sb.AppendLine($"Status Code: {response.StatusCode}");

            AddHeaders(sb, response.Headers);

            await AddContentAsync(sb, response, config.ResponseBodyLogLimit);
            sb.AppendLine("End of Response");

            Console.WriteLine($"{ConsoleColors.ResponseColor}{sb}{ConsoleColors.NormalColor}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error while logging a response to the Console.{Environment.NewLine}{ex}");
        }
    }

    private static async Task AddContentAsync(StringBuilder sb, HttpResponse response, int logBodyLimit)
    {
        sb.AppendLine("Body:");

        if (response.Body.Length == 0)
        {
            sb.AppendLine("[Not present]");
            return;
        }

        using var reader = new StreamReader(response.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Position = 0;

        var size = Math.Min(body.Length, logBodyLimit);
        sb.AppendLine(body.Substring(0, size));
        var isTruncated = size < body.Length;
        if (isTruncated)
        {
            sb.AppendLine($"[Body truncated. Read {logBodyLimit} characters]");
        }
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace HttpServerSim.App.Logger;

public sealed class CustomColorFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private CustomColorOptions _formatterOptions;

    private bool ConsoleColorFormattingEnabled =>
        _formatterOptions.ColorBehavior == LoggerColorBehavior.Enabled ||
        _formatterOptions.ColorBehavior == LoggerColorBehavior.Default &&
        !Console.IsOutputRedirected;

    public CustomColorFormatter(IOptionsMonitor<CustomColorOptions> options)
        // Case insensitive
        : base("customName")
    {
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        _formatterOptions = options.CurrentValue;
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message is null)
        {
            return;
        }

        WriteFormattedLog(logEntry, textWriter, message);
    }

    private void ReloadLoggerOptions(CustomColorOptions options) => _formatterOptions = options;

    private void WriteFormattedLog<TState>(in LogEntry<TState> logEntry, TextWriter textWriter, string message)
    {
        if (!ConsoleColorFormattingEnabled)
        {
            textWriter.Write(logEntry.LogLevel);
            return;
        }

        ConsoleColor foreground;
        string tittle;

        switch (logEntry.LogLevel)
        {
            case LogLevel.Information:
                foreground = ConsoleColor.Green;
                tittle = "Info";
                break;
            case LogLevel.Warning:
                foreground = ConsoleColor.Yellow;
                tittle = "Warn";
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                foreground = ConsoleColor.Red;
                tittle = logEntry.LogLevel.ToString();
                break;
            default:
                foreground = ConsoleColor.Gray;
                tittle = logEntry.LogLevel.ToString();
                break;
        }

        textWriter.WriteWithColor(tittle, ConsoleColor.Black, foreground);
        textWriter.WriteWithColor($" - {logEntry.Category}", ConsoleColor.Black, ConsoleColor.DarkGray);
        textWriter.WriteLine();

        if (logEntry.Category == "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware")
        {
            var isRequestOrResponse = false;
            if (message.StartsWith("Request", StringComparison.InvariantCultureIgnoreCase))
            {
                foreground = ConsoleColor.Cyan;
                isRequestOrResponse = true;
            }
            else if (message.StartsWith("Response", StringComparison.InvariantCultureIgnoreCase))
            {
                foreground = ConsoleColor.DarkCyan;
                isRequestOrResponse = true;
            }

            if (isRequestOrResponse)
            {
                textWriter.WriteWithColor(message, ConsoleColor.Black, foreground);
                textWriter.WriteLine();
                return;
            }
        }

        textWriter.WriteLine(message);
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}
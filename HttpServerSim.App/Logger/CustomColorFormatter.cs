using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace HttpServerSim.App.Logger;

public sealed class CustomColorFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private CustomColorOptions _formatterOptions;
    private readonly ConsoleColor _foregroundRequest;
    private readonly ConsoleColor _foregroundResponse;

    private bool ConsoleColorFormattingEnabled =>
        _formatterOptions.ColorBehavior == LoggerColorBehavior.Enabled ||
        _formatterOptions.ColorBehavior == LoggerColorBehavior.Default &&
        !Console.IsOutputRedirected;

    public CustomColorFormatter(IOptionsMonitor<CustomColorOptions> options)
        // Case insensitive
        : base("CustomColorFormatter")
    {
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        _formatterOptions = options.CurrentValue;

        if (_formatterOptions.IsControlEndpoint)
        {
            _foregroundRequest = ConsoleColor.Magenta;
            _foregroundResponse = ConsoleColor.Magenta;
        }
        else
        {
            _foregroundRequest = ConsoleColor.Cyan;
            _foregroundResponse = ConsoleColor.DarkCyan;
        }
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

    private static void WriteNonRequestOrResponseFormattedLog<TState>(in LogEntry<TState> logEntry, TextWriter textWriter, string message)
    {
        var foreground = logEntry.LogLevel switch
        {
            LogLevel.Information => ConsoleColor.DarkGreen,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error or LogLevel.Critical => ConsoleColor.Red,
            _ => ConsoleColor.Gray,
        };

        textWriter.WriteWithColor(LogLevelToString(logEntry.LogLevel), ConsoleColor.Black, foreground);
        textWriter.WriteLineWithColor($" - {logEntry.Category}{Environment.NewLine}{message}{Environment.NewLine}", ConsoleColor.Black, ConsoleColor.White);
    }

    private static string LogLevelToString(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "error",
            LogLevel.Critical => "crit",
            LogLevel.None => "none",
            _ => throw new InvalidOperationException($"Unexpected {nameof(level)} '{level}'."),
        };
    }

    private void WriteFormattedLog<TState>(in LogEntry<TState> logEntry, TextWriter textWriter, string message)
    {
        if (!ConsoleColorFormattingEnabled)
        {
            textWriter.Write(logEntry.LogLevel);
            return;
        }

        bool isHttpLoggingMiddlewareLog = logEntry.Category == "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware";
        bool isInfo = logEntry.LogLevel == LogLevel.Information;
        bool apearsToBeRequest = isHttpLoggingMiddlewareLog && isInfo && message.StartsWith("Request", StringComparison.InvariantCultureIgnoreCase);
        bool apearsToBeResponse = isHttpLoggingMiddlewareLog && isInfo && message.StartsWith("Response", StringComparison.InvariantCultureIgnoreCase);
        bool apearsToBeDuration = isHttpLoggingMiddlewareLog && isInfo && message.StartsWith("Duration:", StringComparison.InvariantCultureIgnoreCase);

        if (apearsToBeRequest)
        {
            textWriter.WriteLineWithColor($"{message}{Environment.NewLine}", ConsoleColor.Black, _foregroundRequest);
        } 
        else if (apearsToBeResponse)
        {
            textWriter.WriteLineWithColor($"{message}{Environment.NewLine}", ConsoleColor.Black, _foregroundResponse);
        }
        else if (apearsToBeDuration)
        {
            /*
             * Ignore the Duration logs
             * info - Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware
             * Duration: 0.5505ms              
             */
        }
        else
        {
            WriteNonRequestOrResponseFormattedLog(logEntry, textWriter, message);
        }
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}
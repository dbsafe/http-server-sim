namespace HttpServerSim.App.Logger;

public static class ConsoleLoggerExtensions
{
    public static ILoggingBuilder AddCustomColorFormatter(this ILoggingBuilder builder, Action<CustomColorOptions> configure)
    {
        return builder.AddConsole(options => options.FormatterName = "customName")
            .AddConsoleFormatter<CustomColorFormatter, CustomColorOptions>(configure);
    }
}

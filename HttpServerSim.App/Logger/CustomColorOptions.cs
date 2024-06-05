using Microsoft.Extensions.Logging.Console;

namespace HttpServerSim.App.Logger;

public class CustomColorOptions : SimpleConsoleFormatterOptions
{
    // This is not being used for now. It is a way of passing settings to the logger formatter
    public string? CustomPrefix { get; set; }
}

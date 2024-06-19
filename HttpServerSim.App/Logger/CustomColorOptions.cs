using Microsoft.Extensions.Logging.Console;

namespace HttpServerSim.App.Logger;

public class CustomColorOptions : SimpleConsoleFormatterOptions
{
    public bool IsControlEndpoint { get; set; }
}

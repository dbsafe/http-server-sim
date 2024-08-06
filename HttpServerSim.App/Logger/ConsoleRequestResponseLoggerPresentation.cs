// Ignore Spelling: App

using HttpServerSim.App.Contracts;

namespace HttpServerSim;

public class ConsoleRequestResponseLoggerPresentation : IRequestResponseLoggerPresentation
{
    public void LogRequest(string request)
    {
        Console.WriteLine($"{ConsoleColors.RequestColor}{request}{ConsoleColors.NormalColor}");
    }

    public void LogResponse(string response)
    {
        Console.WriteLine($"{ConsoleColors.ResponseColor}{response}{ConsoleColors.NormalColor}");
    }
}

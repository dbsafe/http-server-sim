// Ignore Spelling: App

using HttpServerSim.App.Contracts;
using System.Text;

namespace HttpServerSim;

public class ConsoleRequestResponseLoggerPresentation : IRequestResponseLoggerPresentation
{
    public void LogRequest(string request, string id)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.Append(ConsoleColors.RequestColor);
        
        sb.AppendLine($"{ConsoleColors.Underline}Request:{ConsoleColors.NoUnderline}");
        sb.AppendLine(request);
        sb.Append("End of Request");
        
        sb.Append(ConsoleColors.NormalColor);

        Console.WriteLine(sb.ToString());
    }

    public void LogResponse(string response, string id)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.Append(ConsoleColors.ResponseColor);

        sb.AppendLine($"{ConsoleColors.Underline}Response:{ConsoleColors.NoUnderline}");
        sb.AppendLine(response);
        sb.Append("End of Response");

        sb.Append(ConsoleColors.NormalColor);
        
        Console.WriteLine(sb.ToString());
    }
}

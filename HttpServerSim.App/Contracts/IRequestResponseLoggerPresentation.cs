// Ignore Spelling: App

namespace HttpServerSim.App.Contracts;

public interface IRequestResponseLoggerPresentation
{
    void LogRequest(string request, string id);
    void LogResponse(string response, string id);
}

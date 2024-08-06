// Ignore Spelling: App

namespace HttpServerSim.App.Contracts;

public interface IRequestResponseLoggerPresentation
{
    void LogRequest(string request);
    void LogResponse(string response);
}

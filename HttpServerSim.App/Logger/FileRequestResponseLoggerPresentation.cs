// Ignore Spelling: App

using HttpServerSim.App.Contracts;

namespace HttpServerSim;

public interface IFileRequestResponseLoggerPresentationConfig
{
    string? SaveRequests { get; }
    string? SaveResponses { get; }
}

public class FileRequestResponseLoggerPresentation(IFileRequestResponseLoggerPresentationConfig config, ILogger logger) : IRequestResponseLoggerPresentation
{
    public void LogRequest(string request, string id)
    {
        if (config.SaveRequests is not null)
        {
            var path = Path.Combine(config.SaveRequests, $"{id}.req");
            logger.LogDebug($"Saving {path} ...");
            File.WriteAllText(path, request);
        }
    }

    public void LogResponse(string response, string id)
    {
        if (config.SaveResponses is not null)
        {
            var path = Path.Combine(config.SaveResponses, $"{id}.res");
            logger.LogDebug($"Saving {path} ...");
            File.WriteAllText(path, response);
        }
    }
}


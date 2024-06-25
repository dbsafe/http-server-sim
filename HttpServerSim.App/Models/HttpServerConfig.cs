namespace HttpServerSim.Models;

public class HttpServerConfig(string url, string controlUrl, string[] args)
{
    public string Url { get; } = url;
    public string ControlUrl { get; } = controlUrl;
    public string[] Args { get; } = args;
}

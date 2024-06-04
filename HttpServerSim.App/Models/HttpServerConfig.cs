namespace HttpServerSim.Models;

public class HttpServerConfig(string url, string[] args)
{
    public string Url { get; } = url;
    public string[] Args { get; } = args;
}

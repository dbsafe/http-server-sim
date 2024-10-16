using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Models;

/// <summary>
/// Rule in the server
/// </summary>
/// <param name="name"></param>
public class HttpSimRule(string name) : IHttpSimRule
{
    public string Name { get; } = name;
    public HttpSimResponse? Response { get; set; }
    public IList<HttpSimResponse> Responses { get; set; } = [];
    public DelayRange? Delay { get; set; }

    public List<ConfigCondition>? Conditions { get; set; }
}

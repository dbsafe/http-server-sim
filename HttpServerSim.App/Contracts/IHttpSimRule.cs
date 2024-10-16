using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines a rule in the server.
/// </summary>
public interface IHttpSimRule
{
    string Name { get; }
    HttpSimResponse? Response { get; set; }
    IList<HttpSimResponse> Responses { get; }
    DelayRange? Delay { get; set; }

    // Keep original conditions used to build the RuleEvaluationFunc
    List<ConfigCondition>? Conditions { get; set; }
}

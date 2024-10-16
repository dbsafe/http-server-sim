using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines a rule in the server.
/// </summary>
public interface IHttpSimRule
{
    string Name { get; }
    HttpSimResponse? Response { get; set; }
    DelayRange? Delay { get; set; }

    // Keep original conditions used to build the RuleEvaluationFunc
    public List<ConfigCondition>? Conditions { get; set; }

    // TODO: Move to the rule manager
    IList<HttpSimRequest> Requests { get; }
}

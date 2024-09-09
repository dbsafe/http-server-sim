using HttpServerSim.Client.Models;
using System.Text.Json.Serialization;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines a rule in the server.
/// It has a function with logic based on the condition definition of the rule created by the client.
/// </summary>
public interface IHttpSimRule
{
    string Name { get; }
    HttpSimResponse? Response { get; set; }
    DelayRange? Delay { get; }

    [JsonIgnore]
    Func<HttpSimRequest, bool> RuleEvaluationFunc { get; set; }

    // Keep original conditions used to build the RuleEvaluationFunc
    public List<ConfigCondition>? Conditions { get; set; }

    int MatchCount { get; }
    void IncMatchCount();
    IEnumerable<HttpSimRequest> Requests { get; }
    void AddRequest(HttpSimRequest request);
}

using HttpServerSim.Models;

namespace HttpServerSim.Contracts;

/// <summary>
/// Defines a rule in the server.
/// It has a function with logic based on the condition definition of the rule created by the client.
/// </summary>
public interface IHttpSimRule
{
    string Name { get; }
    HttpSimResponse? Response { get; set; }
    Func<HttpSimRequest, bool> RuleEvaluationFunc { get; set; }
    int MatchCount { get; }
    void IncMatchCount();
    IEnumerable<HttpSimRequest> Requests { get; }
    void AddRequest(HttpSimRequest request);
}

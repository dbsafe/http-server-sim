using HttpServerSim.App.Rules;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines the contract of a rule store in the server
/// </summary>
public interface IHttpSimRuleStore
{
    void CreateRule(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc);
    void UpdateRule(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc);
    IEnumerable<IHttpSimRule> GetRules();
    IHttpSimRule? GetRule(string name);
    void Clear();
    bool DeleteRule(string name);
    int? GetRuleHits(string name);
    HttpSimRequest[]? GetRequests(string name);
    RuleManager? Resolve(HttpSimRequest request);
}

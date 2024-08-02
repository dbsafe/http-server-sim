using HttpServerSim.Contracts;
using HttpServerSim.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Finds a rule that applies to a HTTP request
/// </summary>
/// <param name="ruleStore"></param>
public class HttpSimRuleResolver(IHttpSimRuleStore ruleStore) : IHttpSimRuleResolver
{
    private readonly IHttpSimRuleStore _ruleStore = ruleStore;

    public IHttpSimRule? Resolve(HttpSimRequest request)
    {
        var rule = _ruleStore.GetRules().FirstOrDefault(r => r.RuleEvaluationFunc(request));
        return rule ?? default;
    }
}

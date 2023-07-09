using HttpServerSim.Contracts;
using HttpServerSim.Models;

namespace HttpServerSim.SelfHosted;

public class SelfHostedHttpSimRuleResolver(IHttpSimRuleStore ruleStore) : IHttpSimRuleResolver
{
    private readonly IHttpSimRuleStore _ruleStore = ruleStore;

    public HttpSimRule? Resolve(HttpSimRequest request)
    {
        var rule = _ruleStore.GetRules().FirstOrDefault(r => r.RuleEvaluationFunc(request));
        return rule ?? default;
    }
}

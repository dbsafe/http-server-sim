using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Finds a rule manager that applies to a HTTP request
/// </summary>
/// <param name="ruleStore"></param>
public class HttpSimRuleResolver(IHttpSimRuleStore ruleStore) : IHttpSimRuleResolver
{
    private readonly IHttpSimRuleStore _ruleStore = ruleStore;

    public RuleManager? Resolve(HttpSimRequest request) => _ruleStore.Resolve(request);
}

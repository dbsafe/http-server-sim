using HttpServerSim.Contracts;
using HttpServerSim.Models;
using HttpServerSim.Services;

namespace HttpServerSim.SelfHosted;

public class SelfHostedHttpSimRuleStore : IHttpSimRuleStore
{
    private readonly object _lock = new();

    private readonly Dictionary<string, HttpSimRule> _rules = [];

    public IEnumerable<HttpSimRule> GetRules()
    {
        lock (_lock)
        {
            return [.. _rules.Values];
        }
    }

    public IHttpSimRuleManager CreateRule(string name)
    {
        lock (_lock)
        {
            if (_rules.ContainsKey(name))
            {
                throw new InvalidOperationException("A rule with the same name already exists");
            }

            var ruleBuilder = new HttpSimRuleBuilder(name);
            _rules.Add(name, ruleBuilder.Rule);
            return ruleBuilder;
        }
    }
}

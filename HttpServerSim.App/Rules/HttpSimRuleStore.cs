using HttpServerSim.App.Contracts;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Rule store
/// </summary>
public class HttpSimRuleStore : IHttpSimRuleStore
{
    private readonly object _lock = new();

    private readonly Dictionary<string, IHttpSimRule> _rules = [];

    public IEnumerable<IHttpSimRule> GetRules()
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

    public void Clear()
    {
        lock (_lock)
        {
            _rules.Clear();
        }
    }
}

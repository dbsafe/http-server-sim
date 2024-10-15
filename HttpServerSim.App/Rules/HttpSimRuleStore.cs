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

    public void CreateRule(IHttpSimRule rule)
    {
        lock (_lock)
        {
            if (_rules.ContainsKey(rule.Name))
            {
                throw new InvalidOperationException("A rule with the same name already exists");
            }

            _rules.Add(rule.Name, rule);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _rules.Clear();
        }
    }

    public bool DeleteRule(string name)
    {
        lock (_lock)
        {
            return _rules.Remove(name);
        }
    }
}

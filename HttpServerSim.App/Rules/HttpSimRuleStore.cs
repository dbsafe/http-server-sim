using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Rule store
/// </summary>
public class HttpSimRuleStore : IHttpSimRuleStore
{
    private readonly object _lock = new();

    private readonly Dictionary<string, StoreRuleItemManager> _ruleManagers = [];

    public IEnumerable<IHttpSimRule> GetRules()
    {
        lock (_lock)
        {
            return _ruleManagers.Values.Select(m => m.Rule).ToArray();
        }
    }

    public void CreateRule(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        lock (_lock)
        {
            if (_ruleManagers.ContainsKey(rule.Name))
            {
                throw new InvalidOperationException("A rule with the same name already exists");
            }

            var ruleManager = new StoreRuleItemManager(rule, ruleEvaluationFunc);
            _ruleManagers.Add(rule.Name, ruleManager);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _ruleManagers.Clear();
        }
    }

    public bool DeleteRule(string name)
    {
        lock (_lock)
        {
            return _ruleManagers.Remove(name);
        }
    }

    public void UpdateRule(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        lock (_lock)
        {
            if (_ruleManagers.ContainsKey(rule.Name))
            {
                var ruleManager = new StoreRuleItemManager(rule, ruleEvaluationFunc);
                _ruleManagers[rule.Name] = ruleManager;
                return;
            }

            throw new InvalidOperationException("Rule not found.");
        }
    }

    public IHttpSimRule? GetRule(string name)
    {
        lock (_lock)
        {
            return _ruleManagers.TryGetValue(name, out StoreRuleItemManager? ruleManager) ? ruleManager.Rule : null;
        }
    }

    public int? GetRuleHits(string name)
    {
        lock (_lock)
        {
            return _ruleManagers.TryGetValue(name, out StoreRuleItemManager? ruleManager) ? ruleManager.MatchCount : null;
        }
    }

    public StoreRuleItemManager? Resolve(HttpSimRequest request)
    {
        lock (_lock)
        {
            return _ruleManagers.Values.FirstOrDefault(m => m.RuleEvaluationFunc(request));
        }
    }
}

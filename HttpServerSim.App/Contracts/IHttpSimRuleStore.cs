namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines the contract of a rule store in the server
/// </summary>
public interface IHttpSimRuleStore
{
    void CreateRule(IHttpSimRule rule);
    void UpdateRule(IHttpSimRule rule);
    IEnumerable<IHttpSimRule> GetRules();
    IHttpSimRule? GetRule(string name);
    void Clear();
    bool DeleteRule(string name);
}

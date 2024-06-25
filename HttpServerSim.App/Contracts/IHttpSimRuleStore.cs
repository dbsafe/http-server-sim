namespace HttpServerSim.Contracts;

/// <summary>
/// Defines the contract of a rule store in the server
/// </summary>
public interface IHttpSimRuleStore
{
    IHttpSimRuleManager CreateRule(string name);
    IEnumerable<IHttpSimRule> GetRules();
    void Clear();
}

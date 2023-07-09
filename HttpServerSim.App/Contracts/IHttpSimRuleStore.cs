using HttpServerSim.Models;

namespace HttpServerSim.Contracts;

public interface IHttpSimRuleStore
{
    IHttpSimRuleManager CreateRule(string name);
    IEnumerable<HttpSimRule> GetRules();
}

using HttpServerSim.App.Rules;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines a contract that finds a rule that applies to a HTTP request
/// </summary>
public interface IHttpSimRuleResolver
{
    RuleManager? Resolve(HttpSimRequest request);
}

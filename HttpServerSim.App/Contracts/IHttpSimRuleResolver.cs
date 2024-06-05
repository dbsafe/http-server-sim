using HttpServerSim.Models;

namespace HttpServerSim.Contracts;

/// <summary>
/// Defines a contract that finds a rule that applies to a HTTP request
/// </summary>
public interface IHttpSimRuleResolver
{
    IHttpSimRule? Resolve(HttpSimRequest request);
}

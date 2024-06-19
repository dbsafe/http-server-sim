using HttpServerSim.Models;

namespace HttpServerSim.Contracts;

/// <summary>
/// Defines a contract that supports creating rules in the server.
/// </summary>
public interface IHttpSimRuleManager
{
    IHttpSimRule Rule { get; }

    IHttpSimRuleManager When(Func<HttpSimRequest, bool> ruleEvaluationFunc);

    IHttpSimRuleManager ReturnHttpResponse(HttpSimResponse response);
}

using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines a contract that supports creating rules in the server.
/// </summary>
public interface IHttpSimRuleManager
{
    IHttpSimRule Rule { get; }

    IHttpSimRuleManager When(Func<HttpSimRequest, bool> ruleEvaluationFunc);

    IHttpSimRuleManager ReturnHttpResponse(HttpSimResponse response);
    IHttpSimRuleManager IntroduceDelay(DelayRange? delay);
}

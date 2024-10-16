using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines a contract that supports creating rules in the server.
/// </summary>
public interface IHttpSimRuleBuilder
{
    IHttpSimRule Rule { get; }

    IHttpSimRuleBuilder When(Func<HttpSimRequest, bool> ruleEvaluationFunc);

    IHttpSimRuleBuilder ReturnHttpResponse(HttpSimResponse response);

    IHttpSimRuleBuilder IntroduceDelay(DelayRange? delay);

    Func<HttpSimRequest, bool> RuleEvaluationFunc { get; }
}

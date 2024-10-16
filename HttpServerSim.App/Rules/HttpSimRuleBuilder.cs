using HttpServerSim.App.Contracts;
using HttpServerSim.App.Models;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Supports creating rules in the server.
/// </summary>
/// <param name="name"></param>
public class HttpSimRuleBuilder(string name) : IHttpSimRuleBuilder
{
    public static readonly Func<HttpSimRequest, bool> UnspecifiedRuleEvaluationFunc = _ => false;

    public IHttpSimRule Rule { get; } = new HttpSimRule(name);

    public Func<HttpSimRequest, bool> RuleEvaluationFunc { get; set; } = UnspecifiedRuleEvaluationFunc;

    public IHttpSimRuleBuilder IntroduceDelay(DelayRange? delay)
    {
        EnsureDelayIsNotSet();
        Rule.Delay = delay;
        return this;
    }

    public IHttpSimRuleBuilder ReturnHttpResponse(HttpSimResponse response)
    {
        EnsureResponseIsNotSet();
        Rule.Response = response;
        return this;
    }

    public IHttpSimRuleBuilder When(Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        if (RuleEvaluationFunc != UnspecifiedRuleEvaluationFunc)
        {
            throw new InvalidOperationException($"{nameof(When)} cannot be set more than once");
        }

        RuleEvaluationFunc = ruleEvaluationFunc;
        return this;
    }

    private void EnsureResponseIsNotSet()
    {
        if (Rule.Response != null)
        {
            throw new InvalidOperationException($"{nameof(Rule.Response)} cannot be set more than once");
        }
    }

    private void EnsureDelayIsNotSet()
    {
        if (Rule.Delay != null)
        {
            throw new InvalidOperationException($"{nameof(Rule.Delay)} cannot be set more than once");
        }
    }
}

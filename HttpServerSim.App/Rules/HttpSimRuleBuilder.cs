using HttpServerSim.App.Contracts;
using HttpServerSim.App.Models;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Supports creating rules in the server.
/// </summary>
/// <param name="name"></param>
public class HttpSimRuleBuilder(string name) : IHttpSimRuleManager
{
    public IHttpSimRule Rule { get; } = new HttpSimRule(name);

    public IHttpSimRuleManager IntroduceDelay(DelayRange? delay)
    {
        EnsureDelayIsNotSet();
        Rule.Delay = delay;
        return this;
    }

    public IHttpSimRuleManager ReturnHttpResponse(HttpSimResponse response)
    {
        EnsureResponseIsNotSet();
        Rule.Response = response;
        return this;
    }

    public IHttpSimRuleManager When(Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        if (Rule.RuleEvaluationFunc != HttpSimRule.UnspecifiedRuleEvaluationFunc)
        {
            throw new InvalidOperationException($"{nameof(When)} cannot be set more than once");
        }

        Rule.RuleEvaluationFunc = httpSimRequest =>
        {
            if (ruleEvaluationFunc(httpSimRequest))
            {
                Rule.IncMatchCount();
                return true;
            }

            return false;
        };

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

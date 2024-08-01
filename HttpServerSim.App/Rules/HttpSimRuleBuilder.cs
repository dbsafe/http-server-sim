using HttpServerSim.Contracts;
using HttpServerSim.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Supports creating rules in the server.
/// </summary>
/// <param name="name"></param>
public class HttpSimRuleBuilder(string name) : IHttpSimRuleManager
{
    public IHttpSimRule Rule { get; } = new HttpSimRule(name);

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
            throw new InvalidOperationException("Return cannot be set more than once");
        }
    }
}

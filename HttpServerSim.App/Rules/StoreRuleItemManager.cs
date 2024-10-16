using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

public class StoreRuleItemManager
{
    private long _matchCount;
    private readonly object _requestLocker = new();
    private const int MAX_STORED_REQUESTS = 10;

    public Func<HttpSimRequest, bool> RuleEvaluationFunc { get; }
    public IHttpSimRule Rule { get; }

    public StoreRuleItemManager(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        Rule = rule;
        RuleEvaluationFunc = BuildRuleEvaluationFunc(ruleEvaluationFunc);
    }

    private Func<HttpSimRequest, bool> BuildRuleEvaluationFunc(Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        return httpSimRequest =>
        {
            if (ruleEvaluationFunc(httpSimRequest))
            {
                IncMatchCount();
                return true;
            }

            return false;
        };
    }

    public int MatchCount => (int)Interlocked.Read(ref _matchCount);

    public void AddRequest(HttpSimRequest request)
    {
        lock (_requestLocker)
        {
            Rule.Requests.Add(request);
            while (Rule.Requests.Count > MAX_STORED_REQUESTS)
            {
                Rule.Requests.RemoveAt(0);
            }
        }
    }

    public void IncMatchCount() => Interlocked.Increment(ref _matchCount);
}

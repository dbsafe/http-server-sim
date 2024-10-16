using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

public class RuleManager
{
    private long _matchCount;
    private readonly object _requestLocker = new();
    private const int MAX_STORED_REQUESTS = 10;
    private readonly IList<HttpSimRequest> _requests = [];

    public Func<HttpSimRequest, bool> RuleEvaluationFunc { get; }
    public IHttpSimRule Rule { get; }

    public RuleManager(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc)
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
            _requests.Add(request);
            while (_requests.Count > MAX_STORED_REQUESTS)
            {
                _requests.RemoveAt(0);
            }
        }
    }

    public void IncMatchCount() => Interlocked.Increment(ref _matchCount);

    public IList<HttpSimRequest> Requests 
    { 
        get
        {
            lock (_requestLocker)
            {
                return [.. _requests];
            }
        }
    }
}

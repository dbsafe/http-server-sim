using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;
using System.Net;

namespace HttpServerSim.App.Rules;

public class RuleManager
{
    private long _matchCount;
    private readonly object _requestLocker = new();
    private const int MAX_STORED_REQUESTS = 10;
    private readonly IList<HttpSimRequest> _requests = [];
    private readonly CircularList<HttpSimResponse> _responses;
    private static readonly IList<HttpSimResponse> _defaultResponses = [new() { StatusCode = (int)HttpStatusCode.OK }];

    public Func<HttpSimRequest, bool> RuleEvaluationFunc { get; }
    public IHttpSimRule Rule { get; }

    public RuleManager(IHttpSimRule rule, Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        Rule = rule;
        RuleEvaluationFunc = BuildRuleEvaluationFunc(ruleEvaluationFunc);
        _responses = BuildCircularListOfResponses(rule);
    }

    private static CircularList<HttpSimResponse> BuildCircularListOfResponses(IHttpSimRule rule)
    {
        var responses = rule.Responses.Count == 0 ? _defaultResponses : rule.Responses;
        return new CircularList<HttpSimResponse>([.. responses]);
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

    public HttpSimResponse NextResponse() => _responses.Next();
}

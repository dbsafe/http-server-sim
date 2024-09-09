using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Models;

/// <summary>
/// Rule in the server
/// </summary>
/// <param name="name"></param>
public class HttpSimRule(string name) : IHttpSimRule
{
    private const int MAX_STORED_REQUESTS = 10;
    public static readonly Func<HttpSimRequest, bool> UnspecifiedRuleEvaluationFunc = _ => false;

    private readonly object _requestLocker = new();
    private long _matchCount;
    private readonly List<HttpSimRequest> _requests = [];
    private Func<HttpSimRequest, bool> _ruleEvaluationFunc = UnspecifiedRuleEvaluationFunc;

    public string Name { get; } = name;
    public HttpSimResponse? Response { get; set; }
    public DelayRange? Delay { get; set; }

    public Func<HttpSimRequest, bool> RuleEvaluationFunc
    {
        get => _ruleEvaluationFunc;
        set => _ruleEvaluationFunc = value;
    }

    public int MatchCount => (int)Interlocked.Read(ref _matchCount);
    public IEnumerable<HttpSimRequest> Requests
    {
        get
        {
            lock (_requestLocker)
            {
                return [.. _requests];
            }
        }
    }

    public List<ConfigCondition>? Conditions { get; set; }

    public void IncMatchCount() => Interlocked.Increment(ref _matchCount);
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
}

using HttpServerSim.Contracts;

namespace HttpServerSim.Models;

/// <summary>
/// Rule in the server
/// </summary>
/// <param name="name"></param>
public class HttpSimRule(string name) : IHttpSimRule
{
    public static readonly Func<HttpSimRequest, bool> UnspecifiedRuleEvaluationFunc = _ => false;

    private long _matchCount;
    private readonly List<HttpSimRequest> _requests = [];
    private Func<HttpSimRequest, bool> _ruleEvaluationFunc = UnspecifiedRuleEvaluationFunc;

    public string Name { get; } = name;
    public HttpSimResponse? Response { get; set; }
    public Func<HttpSimRequest, HttpSimResponse>? CreateResponseCallback { get; set; }
    public Action<HttpSimRequest>? Callback { get; set; }
    public Func<HttpSimRequest, bool> RuleEvaluationFunc
    {
        get => _ruleEvaluationFunc;
        set => _ruleEvaluationFunc = value;
    }

    public int MatchCount => (int)Interlocked.Read(ref _matchCount);
    public IEnumerable<HttpSimRequest> Requests => _requests;
    public void IncMatchCount() => Interlocked.Increment(ref _matchCount);
    public void AddRequest(HttpSimRequest request) => _requests.Add(request);
}

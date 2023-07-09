namespace HttpServerSim.Models;

public class HttpSimRule(string name)
{
    internal static readonly Func<HttpSimRequest, bool> UnspecifiedRuleEvaluationFunc = _ => false;

    internal long _matchCount;
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
    public int RuleUsedCount
    {
        get => (int)Interlocked.Read(ref _matchCount);
    }
}

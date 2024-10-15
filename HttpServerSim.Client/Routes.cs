// Ignore Spelling: Json

namespace HttpServerSim.Client;

public static class Routes
{
    public const string CONTROL_ENDPOINT = "/http-server-sim";
    public const string RULES = $"{CONTROL_ENDPOINT}/rules";
    public const string RULE = $"{CONTROL_ENDPOINT}/rules/{{name}}";
    public const string RULE_HITS = $"{RULES}/{{name}}{HITS}";
    public const string HITS = "/hits";
    public const string RULE_REQUESTS = $"{RULES}/{{name}}{REQUESTS}";
    public const string REQUESTS = "/requests";

    public static string RuleByName(string name) => Routes.RULE.Replace("{name}", name);
}

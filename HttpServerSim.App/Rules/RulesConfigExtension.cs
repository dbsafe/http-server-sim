// Ignore Spelling: Api app

using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Defines methods for loading rules in the rule store.
/// </summary>
public static class RulesConfigExtension
{
    public static IApplicationBuilder UseRulesConfig(this WebApplication app, IEnumerable<ConfigRule>? configRules, string responseFilesFolder, IHttpSimRuleStore ruleStore)
    {
        RulesConfigHelper.LoadRules(configRules, responseFilesFolder, ruleStore, app.Logger);
        return app;
    }
}

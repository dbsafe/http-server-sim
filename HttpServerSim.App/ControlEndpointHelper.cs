// Ignore Spelling: Api app

using HttpServerSim.Client;
using HttpServerSim.Client.Models;
using HttpServerSim.Contracts;
using HttpServerSim.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace HttpServerSim;

/// <summary>
/// Implements the control-endpoint
/// </summary>
internal static class ControlEndpointHelper
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, string contentRoot, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapPost(Routes.RULES, ([FromBody] ConfigRule[] rules) =>
        {
            try
            {
                RulesConfigExtension.LoadRules(rules, contentRoot, ruleStore, logger);
                logger.LogDebug($"Created rules '{string.Join(',', rules.Select(r => r.Name))}'");
                return OperationResult.CreateSuccess();
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return OperationResult.CreateFailure(ex.Message);
            }
        });

        app.MapDelete(Routes.RULES, () =>
        {
            try
            {
                ruleStore.Clear();
                logger.LogDebug("Rules deleted");
                return OperationResult.CreateSuccess();
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return OperationResult.CreateFailure(ex.Message);
            }
        });

        app.MapGet(Routes.RULE_HITS, ([FromRoute] string name) =>
        {
            try
            {
                if (!TryGetRule(ruleStore, name, out IHttpSimRule? rule))
                {
                    return OperationResult.CreateFailure($"Rule '{name}' not found.");
                }
                
                return OperationResult.CreateSuccess(rule.MatchCount);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return OperationResult.CreateFailure(ex.Message);
            }
        });

        app.MapGet(Routes.RULE_REQUESTS, ([FromRoute] string name) =>
        {
            try
            {
                if (!TryGetRule(ruleStore, name, out IHttpSimRule? rule))
                {
                    return OperationResult.CreateFailure($"Rule '{name}' not found.");
                }

                return OperationResult.CreateSuccess(rule.Requests);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return OperationResult.CreateFailure(ex.Message);
            }
        });

        return app;
    }

    private static bool TryGetRule(IHttpSimRuleStore ruleStore, string name, [MaybeNullWhen(false)] out IHttpSimRule rule)
    {
        rule = ruleStore.GetRules().FirstOrDefault(x => x.Name == name);
        return rule is not null;
    }
}

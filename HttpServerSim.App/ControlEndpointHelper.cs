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

        app.MapGet(Routes.RULES, () =>
        {
            return ExecuteProtected(logger, () =>
            {
                var rules = ruleStore.GetRules().ToArray();
                return OperationResult.CreateSuccess(rules);
            });
        });

        app.MapGet(Routes.RULE, ([FromRoute] string name) =>
        {
            return ExecuteProtected(logger, () =>
            {
                var rule = ruleStore.GetRules().FirstOrDefault(r => r.Name == name);
                if (rule is null)
                {
                    return OperationResult.CreateFailure($"Rule with name '{name}' not found");
                }
                else
                {
                    return OperationResult.CreateSuccess(rule);
                }
            });
        });

        app.MapPost(Routes.RULES, ([FromBody] ConfigRule[] rules) =>
        {
            return ExecuteProtected(logger, () =>
            {
                RulesConfigExtension.LoadRules(rules, contentRoot, ruleStore, logger);
                logger.LogDebug($"Created rules '{string.Join(',', rules.Select(r => r.Name))}'");
                return OperationResult.CreateSuccess();
            });
        });

        app.MapDelete(Routes.RULES, () =>
        {
            return ExecuteProtected(logger, () =>
            {
                ruleStore.Clear();
                logger.LogDebug("Rules deleted");
                return OperationResult.CreateSuccess();
            });
        });

        app.MapGet(Routes.RULE_HITS, ([FromRoute] string name) =>
        {
            return ExecuteProtected(logger, () =>
            {
                if (!TryGetRule(ruleStore, name, out IHttpSimRule? rule))
                {
                    return OperationResult.CreateFailure($"Rule '{name}' not found.");
                }

                return OperationResult.CreateSuccess(rule.MatchCount);
            });
        });

        app.MapGet(Routes.RULE_REQUESTS, ([FromRoute] string name) =>
        {
            return ExecuteProtected(logger, () =>
            {
                if (!TryGetRule(ruleStore, name, out IHttpSimRule? rule))
                {
                    return OperationResult.CreateFailure($"Rule '{name}' not found.");
                }

                return OperationResult.CreateSuccess(rule.Requests);
            });
        });

        return app;
    }

    private static OperationResult ExecuteProtected(ILogger logger, Func<OperationResult> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            return OperationResult.CreateFailure(ex.Message);
        }
    }

    private static bool TryGetRule(IHttpSimRuleStore ruleStore, string name, [MaybeNullWhen(false)] out IHttpSimRule rule)
    {
        rule = ruleStore.GetRules().FirstOrDefault(x => x.Name == name);
        return rule is not null;
    }
}

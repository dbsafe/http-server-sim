// Ignore Spelling: Api app

using HttpServerSim.App.Contracts;
using HttpServerSim.App.Rules;
using HttpServerSim.Client;
using HttpServerSim.Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace HttpServerSim.App.Middleware;

/// <summary>
/// Implements the control-endpoint
/// </summary>
internal static class ControlEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, string responseFilesFolder, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(app);

        MapGetAllRules(app, ruleStore, logger);
        MapGetRule(app, ruleStore, logger);

        MapCreateRule(app, ruleStore, logger, responseFilesFolder);
        MapDeleteAllRules(app, ruleStore, logger);
        MapDeleteRule(app, ruleStore, logger);
        MapUpdateRule(app, ruleStore, logger, responseFilesFolder);

        MapGetRuleHilts(app, ruleStore, logger);
        MapGetRuleRequests(app, ruleStore, logger);

        return app;
    }

    private static void MapGetAllRules(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger)
    {
        app.MapGet(Routes.RULES, () =>
        {
            return ExecuteProtected(logger, () =>
            {
                var rules = ruleStore.GetRules().ToArray();
                return OperationResult.CreateSuccess(rules);
            });
        })
        .Produces<OperationResult<IHttpSimRule[]>>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get all the rules";
            return operation;
        });
    }

    private static void MapGetRule(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger)
    {
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
        })
        .Produces<OperationResult<IHttpSimRule>>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get one rule by name";
            return operation;
        });
    }

    private static void MapCreateRule(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger, string responseFilesFolder)
    {
        app.MapPost(Routes.RULES, ([FromBody] ConfigRule[] rules) =>
        {
            return ExecuteProtected(logger, () =>
            {
                var createdRules = RulesConfigHelper.LoadRules(rules, responseFilesFolder, ruleStore, logger);
                logger.LogDebug($"Created rules '{string.Join(',', createdRules.Select(r => r.Name))}'.");
                return OperationResult.CreateSuccess();
            });
        })
        .Produces<OperationResult>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Create rules";
            return operation;
        });
    }

    private static void MapUpdateRule(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger, string responseFilesFolder)
    {
        app.MapPut(Routes.RULE, ([FromBody] ConfigRule rule) =>
        {
            return ExecuteProtected(logger, () =>
            {
                var currentRule = ruleStore.GetRule(rule.Name);
                if (currentRule is null)
                {
                    return OperationResult.CreateFailure($"Rule '{rule.Name}' not found.");
                }

                RulesConfigHelper.UpdateRule(rule, responseFilesFolder, ruleStore, logger);
                logger.LogDebug($"Updated rule '{rule.Name}'");
                return OperationResult.CreateSuccess();
            });
        })
        .Produces<OperationResult>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Create rules";
            return operation;
        });
    }

    private static void MapDeleteAllRules(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger)
    {
        app.MapDelete(Routes.RULES, () =>
        {
            return ExecuteProtected(logger, () =>
            {
                ruleStore.Clear();
                logger.LogDebug("Rules deleted");
                return OperationResult.CreateSuccess();
            });
        })
        .Produces<OperationResult>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete all the rules";
            return operation;
        });
    }

    private static void MapDeleteRule(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger)
    {
        app.MapDelete(Routes.RULE, ([FromRoute] string name) =>
        {
            return ExecuteProtected(logger, () =>
            {
                if (ruleStore.DeleteRule(name))
                {
                    logger.LogDebug($"Deleted rule '{name}'");
                    return OperationResult.CreateSuccess();
                }

                return OperationResult.CreateFailure($"Rule with name '{name}' not found");
            });
        })
        .Produces<OperationResult>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete one rule by name";
            return operation;
        });
    }

    private static void MapGetRuleHilts(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger)
    {
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
        })
        .Produces<OperationResult<int>>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the number of requests that matched a rule";
            return operation;
        });
    }

    private static void MapGetRuleRequests(IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, ILogger logger)
    {
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
        })
        .Produces<OperationResult<HttpSimRequest[]>>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the requests that matched a rule";
            return operation;
        });
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

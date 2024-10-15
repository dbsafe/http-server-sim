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

        app.MapPost(Routes.RULES, ([FromBody] ConfigRule[] rules) =>
        {
            return ExecuteProtected(logger, () =>
            {
                RulesConfigHelper.LoadRules(rules, responseFilesFolder, ruleStore, logger);
                logger.LogDebug($"Created rules '{string.Join(',', rules.Select(r => r.Name))}'");
                return OperationResult.CreateSuccess();
            });
        })
        .Produces<OperationResult>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Create rules";
            return operation;
        });

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

        app.MapDelete(Routes.RULE, ([FromRoute] string name) =>
        {
            return ExecuteProtected(logger, () =>
            {
                if (ruleStore.DeleteRule(name))
                {
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

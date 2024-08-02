// Ignore Spelling: Api app

using HttpServerSim.Contracts;
using HttpServerSim.Models;
using System.Text;

namespace HttpServerSim.App.Rules;

/// <summary>
/// Defines methods for loading rules in the rule store.
/// </summary>
public static class RulesConfigExtension
{
    public static IApplicationBuilder UseRulesConfig(this WebApplication app, IEnumerable<ConfigRule>? configRules, string responseFilesFolder, IHttpSimRuleStore ruleStore)
    {
        LoadRules(configRules, responseFilesFolder, ruleStore, app.Logger);
        return app;
    }

    public static void LoadRules(IEnumerable<ConfigRule>? configRules, string responseFilesFolder, IHttpSimRuleStore ruleStore, ILogger logger)
    {
        if (configRules is null || !configRules.Any())
        {
            logger.LogWarning("There is not rules to load.");
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("Loading Rules - Found:");
        foreach (var configRule in configRules)
        {
            sb.AppendLine($"\t{configRule.Name}");

            var apiResponse = BuildResponseFromRule(logger, configRule, responseFilesFolder);
            if (apiResponse is null)
            {
                continue;
            }

            var ruleManager = ruleStore.CreateRule(configRule.Name)
                .When(BuildFuncFromRule(logger, configRule))
                .ReturnHttpResponse(apiResponse);
            ruleManager.Rule.Conditions = configRule.Conditions;
        }

        logger.LogDebug(sb.ToString());
    }

    private static HttpSimResponse? BuildResponseFromRule(ILogger logger, ConfigRule configRule, string responseFilesFolder)
    {
        if (configRule.Response is null)
        {
            logger.LogWarning($"Rule: {configRule.Name} - Missing Response.");
            return null;
        }

        if (configRule.Response.ContentValue is not null)
        {
            switch (configRule.Response.ContentValueType)
            {
                case ContentValueType.Text:
                    break;

                case ContentValueType.File:
                    var path = Path.Combine(responseFilesFolder, configRule.Response.ContentValue);
                    if (!File.Exists(path))
                    {
                        logger.LogWarning($"Rule: {configRule.Name} - File '{path}' not found.");
                        return null;
                    }

                    break;

                default:
                    logger.LogWarning($"Rule: {configRule.Name} - Invalid ContentValueType '{configRule.Response.ContentValueType}'.");
                    return null;
            }
        }

        return configRule.Response;
    }

    private static Func<HttpSimRequest, bool> BuildFuncFromRule(ILogger logger, ConfigRule configRule)
    {
        if (configRule.Conditions is not null && configRule.Conditions.Count >= 0)
        {
            return BuildFuncFromCondition(logger, configRule.Conditions, configRule.Name);
        }

        logger.LogWarning($"Rule: {configRule.Name} - Missing Conditions.");
        return (httpSimRequest) => false;
    }

    private static Func<HttpSimRequest, bool> BuildFuncFromCondition(ILogger logger, IEnumerable<ConfigCondition> configConditions, string ruleName)
    {
        List<Func<HttpSimRequest, bool>> funcs = [];
        foreach (var configCondition in configConditions)
        {
            funcs.Add(BuildFuncFromConditionField(logger, configCondition, ruleName));
        }

        return (httpSimRequest) =>
        {
            if (funcs.Count == 0)
            {
                return false;
            }

            foreach (var func in funcs)
            {
                if (!func.Invoke(httpSimRequest))
                {
                    return false;
                }
            }

            return true;
        };
    }

    private static Func<HttpSimRequest, bool> BuildFuncFromConditionField(ILogger logger, ConfigCondition configCondition, string ruleName)
    {
        if (configCondition.Value is null)
        {
            logger.LogWarning($"Rule: {ruleName} - Missing Value.");
            return (htpSimRequest) => false;
        }

        switch (configCondition.Field)
        {
            case Field.Method:
                return BuildFuncFromOperator(logger, configCondition.Operator, ruleName, (httpSimRequest) => httpSimRequest.Method, configCondition.Field, configCondition.Value);

            case Field.Path:
                return BuildFuncFromOperator(logger, configCondition.Operator, ruleName, (httpSimRequest) => httpSimRequest.Path, configCondition.Field, configCondition.Value);

            default:
                logger.LogWarning($"Rule: {ruleName} - Invalid Field '{configCondition.Field}'.");
                return (htpSimRequest) => false;
        }
    }

    private static Func<HttpSimRequest, bool> BuildFuncFromOperator(ILogger logger, Operator @operator, string ruleName, Func<HttpSimRequest, string> getCurrentValue, Field field, string pattern)
    {
        switch (@operator)
        {
            case Operator.Equals:
                return (httpSimRequest) => string.Compare(getCurrentValue(httpSimRequest), pattern, StringComparison.InvariantCultureIgnoreCase) == 0;

            case Operator.Contains:
                return (httpSimRequest) => pattern is not null && getCurrentValue(httpSimRequest).Contains(pattern, StringComparison.InvariantCultureIgnoreCase);

            case Operator.StartWith:
                return (httpSimRequest) => pattern is not null && getCurrentValue(httpSimRequest).StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase);

            default:
                logger.LogWarning($"Rule: {ruleName}, Condition Field: {field} - Invalid Operator '{@operator}'");
                return (httpSimRequest) => false;
        }
    }
}

﻿// Ignore Spelling: Api app

using HttpServerSim.App.Contracts;
using HttpServerSim.Client.Models;
using System.Text;

namespace HttpServerSim.App.Rules;

public static class RulesConfigHelper
{
    public static IEnumerable<ConfigRule> LoadRules(IEnumerable<ConfigRule>? configRules, string responseFilesFolder, IHttpSimRuleStore ruleStore, ILogger logger)
    {
        if (configRules is null || !configRules.Any())
        {
            logger.LogWarning("There are not rules to load.");
            return [];
        }

        var createdRules = new List<ConfigRule>();
        StringBuilder sb = new();
        sb.AppendLine("Loading Rules - Found:");
        foreach (var configRule in configRules)
        {
            sb.AppendLine($"\t{configRule.Name}");
            if (CreateRule(configRule, responseFilesFolder, ruleStore, logger))
            {
                createdRules.Add(configRule);
            }
        }

        logger.LogDebug(sb.ToString());
        return createdRules;
    }

    private static bool CreateRule(ConfigRule configRule, string responseFilesFolder, IHttpSimRuleStore ruleStore, ILogger logger) =>
        ActionWithRule(configRule, responseFilesFolder, logger, ruleStore.CreateRule);

    public static bool UpdateRule(ConfigRule configRule, string responseFilesFolder, IHttpSimRuleStore ruleStore, ILogger logger) =>
        ActionWithRule(configRule, responseFilesFolder, logger, ruleStore.UpdateRule);

    private static bool ActionWithRule(ConfigRule configRule, string responseFilesFolder, ILogger logger, Action<IHttpSimRule, Func<HttpSimRequest, bool>> action)
    {
        if (configRule.Response is not null && configRule.Responses.Count > 0)
        {
            logger.LogWarning($"Rule: {configRule.Name} - Only property '{nameof(configRule.Response)}' or '{nameof(configRule.Responses)}' can have responses but not both.");
            return false;
        }

        if (configRule.Response is not null)
        {
            configRule.Responses.Add(configRule.Response);
            configRule.Response = null;
        }

        if (!ValidateRuleResponses(logger, configRule.Responses, configRule.Name, responseFilesFolder))
        {
            return false;
        }

        var httpSimRuleBuilder = CreateHttpSimRuleBuilder(configRule, logger, configRule.Responses);
        action.Invoke(httpSimRuleBuilder.Rule, httpSimRuleBuilder.RuleEvaluationFunc);
        return true;
    }

    private static IHttpSimRuleBuilder CreateHttpSimRuleBuilder(ConfigRule configRule, ILogger logger, IList<HttpSimResponse> apiResponses)
    {
        var builder = new HttpSimRuleBuilder(configRule.Name)
            .When(BuildFuncFromRule(logger, configRule));
        
        foreach (var response in apiResponses)
        {
            builder.ReturnHttpResponse(response);
        }

        builder.IntroduceDelay(configRule.Delay);

        builder.Rule.Conditions = configRule.Conditions;
        return builder;
    }

    public static bool ValidateRuleResponses(ILogger logger, IList<HttpSimResponse> responses, string ruleName, string responseFilesFolder)
    {
        if (responses.Count == 0)
        {
            logger.LogWarning($"Rule: {ruleName} - Missing Response.");
            return false;
        }

        var areResponsesValid = true;
        foreach (var response in responses)
        {
            if (response.ContentValue is null)
            {
                continue;
            }

            switch (response.ContentValueType)
            {
                case ContentValueType.Text:
                    break;

                case ContentValueType.File:
                    var path = Path.Combine(responseFilesFolder, response.ContentValue);
                    if (!File.Exists(path))
                    {
                        logger.LogWarning($"Rule: {ruleName} - File '{path}' not found.");
                        areResponsesValid = false;
                    }

                    break;

                default:
                    logger.LogWarning($"Rule: {ruleName} - Invalid ContentValueType '{response.ContentValueType}'.");
                    areResponsesValid = false;
                    break;
            }
        }

        return areResponsesValid;
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

// Ignore Spelling: Json

using HttpServerSim.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HttpServerSim.Client;

/// <summary>
/// Support for building rules using a fluent interface  
/// </summary>
public class RuleBuilder
{
    public ConfigRule Rule { get; }

    private RuleBuilder()
    {
        Rule = new ConfigRule();
    }

    public static RuleBuilder CreateRule(string name, string? description = null)
    {
        var ruleBuilder = new RuleBuilder();
        ruleBuilder.Rule.Name = name;
        ruleBuilder.Rule.Description = description;
        return ruleBuilder;
    }
}

public static class RuleBuilderExtensionMethods
{
    public static RuleBuilder WithCondition(this RuleBuilder ruleBuilder, Field field, Operator op, string value)
    {
        ruleBuilder.Rule.Conditions.Add(new ConfigCondition { Field = field, Operator = op, Value = value });
        return ruleBuilder;
    }

    public static RuleBuilder WithConditions(this RuleBuilder ruleBuilder, IEnumerable<ConfigCondition> conditions)
    {
        ruleBuilder.Rule.Conditions.AddRange(conditions);
        return ruleBuilder;
    }

    public static RuleBuilder WithResponse(this RuleBuilder ruleBuilder, HttpSimResponse response)
    {
        ruleBuilder.Rule.Response = response;
        return ruleBuilder;
    }

    public static RuleBuilder WithJsonResponse<TBody>(this RuleBuilder ruleBuilder, TBody content, string contentType = "application/json", KeyValuePair<string, string[]>[]? headers = null, int statusCode = 200, HttpSimResponseEncoding encoding = HttpSimResponseEncoding.None)
    {
        var jsonContent = JsonSerializer.Serialize(content);
        return WithTextResponse(ruleBuilder, jsonContent, contentType, headers, statusCode, encoding);
    }

    public static RuleBuilder WithTextResponse(this RuleBuilder ruleBuilder, string content, string contentType = "text/plain", KeyValuePair<string, string[]>[]? headers = null, int statusCode = 200, HttpSimResponseEncoding encoding = HttpSimResponseEncoding.None)
    {
        var response = new HttpSimResponse { StatusCode = statusCode, ContentValue = content, ContentType = contentType, Headers = headers, Encoding = encoding };
        return WithResponse(ruleBuilder, response);
    }

    public static RuleBuilder ReturnTextResponseFromFile(this RuleBuilder ruleBuilder, string path, string contentType = "application/json", KeyValuePair<string, string[]>[]? headers = null, int statusCode = 200)
    {
        var response = new HttpSimResponse
        {
            StatusCode = statusCode,
            ContentValue = path,
            ContentType = contentType,
            ContentValueType = ContentValueType.File,
            Headers = headers
        };

        return WithResponse(ruleBuilder, response);
    }

    public static RuleBuilder ReturnWithStatusCode(this RuleBuilder ruleBuilder, int statusCode, KeyValuePair<string, string[]>[]? headers = null)
    {
        var response = new HttpSimResponse { StatusCode = statusCode, Headers = headers };
        return WithResponse(ruleBuilder, response);
    }
}

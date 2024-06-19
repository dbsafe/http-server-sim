using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HttpServerSim.Models;

/// <summary>
/// Defines a schema used when loading rules from a json file
/// </summary>
public class RulesConfig
{
    public ConfigRule[]? Rules { get; set; }
}

/// <summary>
/// Defines a rule for the client side
/// </summary>
public class ConfigRule
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ConfigCondition> Conditions { get; set; } = [];
    public HttpSimResponse? Response { get; set; }
}

/// <summary>
/// Defines a condition. 
/// Conditions are used in the server to create a function used when searching for a rule that matches a HTTP request.
/// </summary>
public class ConfigCondition
{
    public Field Field { get; set; }
    public Operator Operator { get; set; }
    public string? Value { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Field
{
    Method,
    Path
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Operator
{
    Equals,
    StartWith,
    Contains
}

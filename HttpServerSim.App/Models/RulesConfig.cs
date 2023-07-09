using System.Text.Json.Serialization;

namespace HttpServerSim.Models;

public class RulesConfig
{
    public ConfigRule[]? Rules { get; set; }
}

public class ConfigRule
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ConfigCondition[]? Conditions { get; set; }
    public HttpSimResponse? Response { get; set; }
}

public class ConfigCondition
{
    public Field Field { get; set; }
    public Operator Operator { get; set; }
    public string? Value { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<Field>))]
public enum Field
{
    Method,
    Path
}

[JsonConverter(typeof(JsonStringEnumConverter<Operator>))]
public enum Operator
{
    Equals,
    StartWith,
    Contains
}

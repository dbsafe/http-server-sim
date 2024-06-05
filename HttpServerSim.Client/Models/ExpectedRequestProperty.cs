namespace HttpServerSim.Client.Models;

public class ExpectedRequestProperty
{
    public string? Name { get; set; }
    public object? Value { get; set; }
    public ExpectedRequestPropertyType Type { get; set; }
}

public enum ExpectedRequestPropertyType
{
    Text,
    Number,
    DateTime,
    Guid
}
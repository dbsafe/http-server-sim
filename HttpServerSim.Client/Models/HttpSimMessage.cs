using System.Collections.Generic;

namespace HttpServerSim.Client.Models;

/// <summary>
/// Defines a base HTTP message
/// </summary>
public abstract class HttpSimMessage
{
    public KeyValuePair<string, string[]>[]? Headers { get; set; }
    public string? ContentValue { get; set; }
    public string? ContentType { get; set; }
}

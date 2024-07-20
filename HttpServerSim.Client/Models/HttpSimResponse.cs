using System.Text.Json.Serialization;

namespace HttpServerSim.Models;

/// <summary>
/// Defines a response HTTP message
/// </summary>
public class HttpSimResponse : HttpSimMessage
{
    public int StatusCode { get; set; }
    public HttpSimResponseEncoding Encoding { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HttpSimResponseEncoding
{
    None,
    GZip
}
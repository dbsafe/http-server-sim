namespace HttpServerSim.Models;

/// <summary>
/// Defines a response HTTP message
/// </summary>
public class HttpSimResponse : HttpSimMessage
{
    public int StatusCode { get; set; }
}

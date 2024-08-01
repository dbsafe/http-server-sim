using System.Collections.Generic;

namespace HttpServerSim.Models
{
    /// <summary>
    /// Defines a base HTTP message
    /// </summary>
    public abstract class HttpSimMessage
    {
        public KeyValuePair<string, string[]>[]? Headers { get; set; }
        public string? ContentValue { get; set; }
        public string? ContentType { get; set; }
    }
}

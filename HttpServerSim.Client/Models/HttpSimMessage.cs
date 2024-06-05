using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HttpServerSim.Models
{
    /// <summary>
    /// Defines a base HTTP message
    /// </summary>
    public abstract class HttpSimMessage
    {
        public KeyValuePair<string, string[]>[]? Headers { get; set; }
        public string? ContentValue { get; set; }
        public ContentValueType ContentValueType { get; set; }
        public string? ContentType { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentValueType
    {
        Text,
        File
    }
}

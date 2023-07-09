using System.Text.Json.Serialization;

namespace HttpServerSim.Models
{
    public class HttpSimMessage
    {
        public KeyValuePair<string, string[]>[]? Headers { get; set; }
        public string? ContentValue { get; set; }
        public ContentValueType ContentValueType { get; set; }
        public string? ContentType { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<ContentValueType>))]
    public enum ContentValueType
    {
        Text,
        File
    }
}

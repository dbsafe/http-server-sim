// Ignore Spelling: App

using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerSim.App.Config;

public class AppConfig
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string? Rules { get; set; }
    public string? Url { get; set; } = "http://localhost:5000";
    public string? ControlUrl { get; set; }
    public bool LogControlRequestAndResponse { get; set; }
    public bool LogRequestAndResponse { get; set; } = true;
    public string CurrentDirectory { get; } = Environment.CurrentDirectory;

    [JsonIgnore]
    public string? ResponseFilesFolder { get; set; }
    [JsonIgnore]
    public bool IsDebugMode { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, _jsonSerializerOptions);

    public static string[] ExpectedOptions { get; } = [nameof(Rules), nameof(Url)];
}

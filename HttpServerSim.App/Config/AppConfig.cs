// Ignore Spelling: App

using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerSim.App.Config;

public class AppConfig : IConsoleRequestResponseLoggerConfig
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
    public int RequestBodyLogLimit { get; set; } = 4096;
    public int ResponseBodyLogLimit { get; set; } = 4096;

    [JsonIgnore]
    public string? ResponseFilesFolder { get; set; }
    [JsonIgnore]
    public bool IsDebugMode { get; set; }

    // This is used when logging the current configuration
    public override string ToString() => JsonSerializer.Serialize(this, _jsonSerializerOptions);
}

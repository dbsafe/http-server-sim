// Ignore Spelling: App

using System.Configuration;
using System.Reflection;
using System.Text.Json;

namespace HttpServerSim;

public class AppConfig
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public string? RulesPath { get; set; }
    public string? Url { get; set; }
    public string? ControlUrl { get; set; }
    public bool LogControlRequestAndResponse { get; set; }
    public bool LogRequestAndResponse { get; set; }
    public string? ResponseFilesFolder { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, _jsonSerializerOptions);
}

public class AppConfigLoader
{
    private const string CONFIG_APP_NAME = "HttpServerSim";

    public static AppConfig Load(IConfiguration config)
    {
        AppConfig appConfig = config.GetSection(CONFIG_APP_NAME).Get<AppConfig>() ?? throw new ConfigurationErrorsException("Failed to load configuration");
        if (!Validate(appConfig, out string message))
        {
            throw new ConfigurationErrorsException(message);
        }

        appConfig.RulesPath = NormalizePath(appConfig.RulesPath, nameof(appConfig.RulesPath));
        return appConfig!;
    }

    private static string NormalizePath(string? path, string configName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ConfigurationErrorsException($"Invalid {configName}.");
        }

        if (File.Exists(path))
        {
            return path;
        }

        var newPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        if (File.Exists(newPath))
        {
            return newPath;
        }

        newPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, path);
        if (File.Exists(newPath))
        {
            return newPath;
        }

        throw new ConfigurationErrorsException($"Invalid {configName} '{path}'.");
    }

    private static bool Validate(AppConfig appConfig, out string message)
    {
        if (string.IsNullOrWhiteSpace(appConfig.RulesPath))
        {
            message = $"Invalid {nameof(appConfig.RulesPath)}.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(appConfig.Url))
        {
            message = $"Invalid {nameof(appConfig.Url)}.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}


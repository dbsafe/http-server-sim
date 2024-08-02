// Ignore Spelling: App

using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HttpServerSim.App.Config;

public class AppConfigLoader
{
    public static void PrintHelp()
    {
        static string PadRight(string source) => source.PadRight(35);

        var sb = new StringBuilder();
        sb.AppendLine("Usage: http-server-sim [options...]");

        sb.AppendLine($"{PadRight("--ControlUrl <url>")}URL for managing rules dynamically. Not required. Example: http://localhost:5001.");
        sb.AppendLine($"{PadRight("--Help")}Prints this help.");
        sb.AppendLine($"{PadRight("--LogControlRequestAndResponse")}Whether control requests and responses are logged. Default: false.");
        sb.AppendLine($"{PadRight("--LogRequestAndResponse")}Whether requests and responses are logged. Default: true.");
        sb.AppendLine($"{PadRight("--Rules <file-name> | <path>")}Rules file. It can be a file name of a file that exists in the current directory or a full path to a file.");
        sb.AppendLine($"{PadRight("--Url <url>")}URL for simulating endpoints. Default: http://localhost:5000.");
        sb.AppendLine($"{PadRight("")}--Url and --ControlUrl cannot share the same value.");

        Console.WriteLine(sb.ToString());
    }

    public static bool TryLoadAppConfig(string[] args, bool isDebugMode, [NotNullWhen(true)] out AppConfig? appConfig)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();

        try
        {
            appConfig = Load(builder.Configuration);
            appConfig.IsDebugMode = isDebugMode;
            Console.WriteLine($"Configuration:{Environment.NewLine}{appConfig}");
            return true;
        }
        catch (ConfigurationErrorsException ex)
        {
            var error = isDebugMode ? ex.ToString() : ex.Message;
            Console.WriteLine($"Failed to load configuration.{Environment.NewLine}{error}");

            appConfig = null;
            return false;
        }
    }

    public static AppConfig Load(IConfiguration config)
    {
        AppConfig appConfig = config.Get<AppConfig>() ?? throw new ConfigurationErrorsException("Missing arguments and configuration");
        Validate(appConfig);
        return appConfig!;
    }

    private static string? NormalizeRulesPath(string? path, string currentDirectory)
    {
        // RulesPath is optional
        if (path is null)
        {
            return null;
        }

        var result = Path.Combine(currentDirectory, path);
        if (File.Exists(result))
        {
            return result;
        }

        if (File.Exists(path))
        {
            return path;
        }

        throw new ConfigurationErrorsException($"File '{path}' not found");
    }

    private static void Validate(AppConfig appConfig)
    {
        appConfig.Rules = NormalizeRulesPath(appConfig.Rules, appConfig.CurrentDirectory);
        if (string.IsNullOrWhiteSpace(appConfig.Url))
        {
            throw new ConfigurationErrorsException($"Invalid {nameof(appConfig.Url)}.");
        }

        // Use current directory for now. In the future it could be passed as an option and fall back to the current directory
        appConfig.ResponseFilesFolder = appConfig.CurrentDirectory;
    }
}


// Ignore Spelling: App

using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace HttpServerSim.App.Config;

public class AppConfigLoader
{
    public static Dictionary<string, string> ValidArgs { get; } = GetValidArgs();

    private static Dictionary<string, string> GetValidArgs()
    {
        return typeof(AppConfig).GetProperties()
            .Where(p => p.GetCustomAttribute(typeof(JsonIgnoreAttribute)) is null)
            .Select(p => p.Name)
            .ToDictionary(n => n) ?? [];
    }

    public static void PrintHelp()
    {
        static string PadRight(string source) => source.PadRight(35);

        var sb = new StringBuilder();
        sb.AppendLine("Usage: http-server-sim [options...]");

        sb.AppendLine($"{PadRight("--ControlUrl <url>")}URL for managing rules dynamically. Not required. Example: http://localhost:5001.");

        sb.AppendLine($"{PadRight("--DefaultContentType <value>")}The Content-Type used in a response message when no rule matching the request is found.");
        sb.AppendLine($"{PadRight("--DefaultContentValue <value>")}The Content used in a response message when no rule matching the request is found.");

        sb.AppendLine($"{PadRight("--DefaultDelayMin <value>")}The delay (in milliseconds) before sending a default response message when no matching rule for the request is found. Default: 0.");
        sb.AppendLine($"{PadRight("--DefaultDelayMax <value>")}The maximum delay (in milliseconds) before sending a default response message when no matching rule for the request is found.");
        sb.AppendLine($"{PadRight("")}When --DefaultDelayMax is specified, the actual delay will be a random value between --DefaultDelayMin and --DefaultDelayMax.");

        sb.AppendLine($"{PadRight("--DefaultStatusCode <value>")}The HTTP status code used in a response message when no rule matching the request is found. Default: 200.");

        sb.AppendLine($"{PadRight("--Help")}Prints this help.");

        sb.AppendLine($"{PadRight("--LogControlRequestAndResponse")}Whether control requests and responses are logged. Default: false.");
        sb.AppendLine($"{PadRight("--LogRequestAndResponse")}Whether requests and responses are logged. Default: true.");

        sb.AppendLine($"{PadRight("--RequestBodyLogLimit <limit>")}Maximum request body size to log (in bytes). Default: 4096.");
        sb.AppendLine($"{PadRight("--ResponseBodyLogLimit <limit>")}Maximum response body size to log (in bytes). Default: 4096.");

        sb.AppendLine($"{PadRight("--Rules <file-name> | <path>")}Rules file. It can be a file name of a file that exists in the current directory or a full path to a file.");

        sb.AppendLine($"{PadRight("--SaveRequests <directory>")}The directory where request messages are saved.");
        sb.AppendLine($"{PadRight("--SaveResponses <directory>")}The directory where response messages are saved.");

        sb.AppendLine($"{PadRight("--Url <url>")}URL for simulating endpoints. Default: http://localhost:5000.");

        sb.AppendLine($"{PadRight("")}--Url and --ControlUrl cannot share the same value.");

        Console.WriteLine(sb.ToString());
    }

    public static bool TryLoadAppConfig(string[] args, bool isDebugMode, [NotNullWhen(true)] out AppConfig? appConfig)
    {
        if (!ValidateArgs(args))
        {
            appConfig = null;
            return false;
        }

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

    private static bool ValidateArgs(string[] args)
    {
        var parsedArgs = CommandLineHelper.ParseArgs(args);

        var validArgs = GetValidArgs();
        var invalidArgs = parsedArgs.Select(a => a.Key).Where(a => !a.StartsWith("Logging:") && !validArgs.TryGetValue(a, out _));
        if (!invalidArgs.Any())
        {
            return true;
        }

        var optionWord = invalidArgs.Count() == 1 ? "option" : "options";
        var sb = new StringBuilder();
        if (invalidArgs.Count() == 1)
        {
            sb.AppendLine("Invalid option:");
        }
        else
        {
            sb.AppendLine("Invalid options:");
        }

        foreach (var invalidArg in invalidArgs)
        {
            sb.AppendLine($"\t{invalidArg}");
        }

        Console.WriteLine(sb);
        return false;
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

    private static string? NormalizeDirectory(string? directory)
    {
        if (directory is null)
        {
            return null;
        }

        var fullDirectory = Path.IsPathRooted(directory) ? directory : Path.GetFullPath(directory);
        if (!Directory.Exists(fullDirectory))
        {
            try
            {
                Directory.CreateDirectory(fullDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory '{fullDirectory}'.{Environment.NewLine}{ex}");
            }
        }

        return fullDirectory;
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

        appConfig.SaveResponses = NormalizeDirectory(appConfig.SaveResponses);
        appConfig.SaveRequests = NormalizeDirectory(appConfig.SaveRequests);
    }
}


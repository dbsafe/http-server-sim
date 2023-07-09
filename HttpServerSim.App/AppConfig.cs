// Ignore Spelling: App

using System.Configuration;
using System.Reflection;
using System.Text;

namespace HttpServerSim;

public class AppConfig
{
    public string? RulesPath { get; set; }
    public string? Url { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine($"{nameof(RulesPath)}: {RulesPath}");
        sb.AppendLine($"{nameof(Url)}: {Url}");

        return sb.ToString();
    }
}

public class AppConfigLoader
{
    private const string CONFIG_APP_NAME = "HttpServerSim";

    public static AppConfig Load(string[] args)
    {
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddJsonFile("appsettings.json");
        configurationBuilder.AddCommandLine(args);
        IConfigurationRoot config = configurationBuilder.Build();

        AppConfig appConfig = new()
        {
            RulesPath = config[$"{CONFIG_APP_NAME}:{nameof(AppConfig.RulesPath)}"],
            Url = config[$"{CONFIG_APP_NAME}:{nameof(AppConfig.Url)}"],
        };

        if (!Validate(appConfig, out string message))
        {
            throw new ConfigurationErrorsException(message);
        }

        return appConfig;
    }

    private static bool Validate(AppConfig appConfig, out string message)
    {
        if (string.IsNullOrWhiteSpace(appConfig.RulesPath))
        {
            message = $"Invalid {nameof(appConfig.RulesPath)}.";
            return false;
        }

        if (string.IsNullOrEmpty(Path.GetDirectoryName(appConfig.RulesPath)))
        {
            var actualPath = Path.Combine(Directory.GetCurrentDirectory(), appConfig.RulesPath);
            if (File.Exists(actualPath))
            {
                appConfig.RulesPath = actualPath;
            }
            else
            {
                actualPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, appConfig.RulesPath);
                if (File.Exists(actualPath))
                {
                    appConfig.RulesPath = actualPath;
                }
            }
        }

        if (!File.Exists(appConfig.RulesPath))
        {
            message = $"File '{appConfig.RulesPath}' not found.";
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


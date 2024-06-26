// Ignore Spelling: Api app

using HttpServerSim.App.Logger;
using HttpServerSim.Contracts;
using HttpServerSim.Models;
using HttpServerSim.SelfHosted;
using Microsoft.AspNetCore.HttpLogging;
using System.Configuration;
using System.Text.Json;

namespace HttpServerSim;

public sealed class ApiHttpSimServer : IDisposable
{
    private WebApplication? _httpSimApp;
    private WebApplication? _controlApp;
    private readonly IHttpSimRuleResolver _httpSimRuleResolver;
    private readonly RulesConfig rulesConfig;

    private readonly HttpSimRuleStore _ruleStore = new();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiHttpSimServer(string[] args)
    {
        _httpSimRuleResolver = new HttpSimRuleResolver(_ruleStore);
        (_httpSimApp, var appConfig) = BuildHttpSimApplication(args, isControlEndpoint: false, appConfig => appConfig.LogRequestAndResponse);
        _httpSimApp.UseHttpSimRuleResolver(_httpSimRuleResolver, _httpSimApp.Logger);
        rulesConfig = LoadRulesConfig(appConfig);

        _httpSimApp.UseRulesConfig(rulesConfig.Rules, Path.GetDirectoryName(appConfig.RulesPath)!, _ruleStore);
        _httpSimApp.Urls.Add(appConfig.Url!);

        // Start control endpoint only when the url is present
        if (!string.IsNullOrEmpty(appConfig.ControlUrl))
        {
            (_controlApp, _) = BuildHttpSimApplication(args, isControlEndpoint: true, appConfig => appConfig.LogControlRequestAndResponse);
            _controlApp.MapControlEndpoints(_ruleStore, _controlApp.Environment.ContentRootPath, _controlApp.Logger);
            _controlApp.Urls.Add(appConfig.ControlUrl!);
        }
    }

    private (WebApplication, AppConfig) BuildHttpSimApplication(string[] args, bool isControlEndpoint, Func<AppConfig, bool> getLogRequestAndResponse)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();

        var appConfig = LoadAppConfig(builder.Configuration);

        if (getLogRequestAndResponse.Invoke(appConfig))

        {
            AddCustomColorFormatter(builder, isControlEndpoint);
        }
        else
        {
            builder.Logging.AddConsole();
        }

        var app = builder.Build();
        if (getLogRequestAndResponse.Invoke(appConfig))
        {
            app.UseHttpLogging();
        }
        
        return (app, appConfig);
    }

    private static void AddCustomColorFormatter(WebApplicationBuilder builder, bool isControlEndpoint)
    {
        builder.Logging.AddCustomColorFormatter(options =>
        {
            options.IsControlEndpoint = isControlEndpoint;
        });

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All & ~(HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders);
            //logging.LoggingFields = HttpLoggingFields.All;
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            //logging.CombineLogs = true;
        });

        builder.Services.AddHttpLoggingInterceptor<HttpLoggingInterceptor>();
    }

    public void Dispose()
    {
        DisposeWebApplication(_httpSimApp);
        _httpSimApp = null;
        DisposeWebApplication(_controlApp);
        _controlApp = null;
    }

    private static void DisposeWebApplication(WebApplication? application)
    {
        if (application != null)
        {
            Task.Run(async () =>
            {
                await application.DisposeAsync();
            }).Wait();
        }
    }

    public void Run()
    {
        Task[] tasks =
        [
            Task.Run(() => _httpSimApp?.Run()),
            Task.Run(() => _controlApp?.Run())
        ];

        Task.WaitAll(tasks);
    }

    public void Start()
    {
        _httpSimApp!.Start();
        _controlApp?.Start();
    }

    public void Stop()
    {
        _httpSimApp?.StopAsync().GetAwaiter().GetResult();
        _controlApp?.StopAsync().GetAwaiter().GetResult();
    }

    public IHttpSimRuleManager CreateRule(string name) => _ruleStore.CreateRule(name);

    private static AppConfig LoadAppConfig(IConfiguration config)
    {
        try
        {
            var appConfig = AppConfigLoader.Load(config);
            Console.WriteLine($"Configuration:{Environment.NewLine}{appConfig}");
            return appConfig;
        }
        catch (ConfigurationErrorsException)
        {
            Console.WriteLine("Failed to load configuration.");
            throw;
        }
    }

    private static RulesConfig LoadRulesConfig(AppConfig appConfig)
    {
        try
        {
            using var fileStream = File.OpenRead(appConfig.RulesPath!);
            return JsonSerializer.Deserialize<RulesConfig>(fileStream, _jsonSerializerOptions) ?? throw new InvalidDataException($"Failed to load {nameof(RulesConfig)} from '{appConfig.RulesPath}'");
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to load rules.");
            throw;
        }
    }
}

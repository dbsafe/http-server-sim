// Ignore Spelling: Api app

using HttpServerSim.Contracts;
using HttpServerSim.Models;
using HttpServerSim.SelfHosted;
using Microsoft.AspNetCore.HttpLogging;
using System.Configuration;
using System.Text.Json;

namespace HttpServerSim;

public sealed class ApiHttpSimServer : IDisposable
{
    private WebApplication? _app;
    private readonly IHttpSimRuleResolver _httpSimRuleResolver;

    private readonly SelfHostedHttpSimRuleStore _ruleStore = new();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() 
    { 
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
    };

    public ApiHttpSimServer(string[] args)
    {
        _httpSimRuleResolver = new SelfHostedHttpSimRuleResolver(_ruleStore);
        _app = BuildApplication(args);

        var appConfig = LoadAppConfig(args, _app.Logger);
        var rulesConfig = LoadRulesConfig(appConfig);
        _app.UseRulesConfig(rulesConfig.Rules, Path.GetDirectoryName(appConfig.RulesPath)!, _ruleStore);

        _app.Urls.Add(appConfig.Url!);
    }

    // Allows passing the configuration. Used for testing
    public ApiHttpSimServer(HttpServerConfig config)
    {
        _httpSimRuleResolver = new SelfHostedHttpSimRuleResolver(_ruleStore);
        _app = BuildApplication(config.Args);

        _app.Urls.Add(config.Url);
    }

    private WebApplication BuildApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All & ~(HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders);
            //logging.LoggingFields = HttpLoggingFields.All;
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
        });

        builder.Services.AddHttpLoggingInterceptor<HttpLoggingInterceptor>();

        var app = builder.Build();
        app.UseHttpLogging();

        app.MapGet("/control-endpoint", () => "This is the control endpoint");
        app.UseHttpSimRuleResolver(_httpSimRuleResolver, app.Logger);
        return app;
    }

    public void Dispose()
    {
        if (_app != null)
        {
            Task.Run(async () =>
            {
                await _app.DisposeAsync();
            }).Wait();

            _app = null;
        }
    }

    public void Run()
    {
        _app?.Run();
    }

    public void Start()
    {
        _app!.Start();
    }

    public void Stop()
    {
        _app?.StopAsync().GetAwaiter().GetResult();
    }

    public IHttpSimRuleManager CreateRule(string name) => _ruleStore.CreateRule(name);

    private static AppConfig LoadAppConfig(string[] args, ILogger logger)
    {
        try
        {
            var appConfig = AppConfigLoader.Load(args);
            logger.LogInformation($"Configuration:{Environment.NewLine}{appConfig}");
            return appConfig;
        }
        catch (ConfigurationErrorsException)
        {
            logger.LogError("Failed to load configuration.");
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

public sealed class HttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        AddHeaders(logContext);
        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        AddHeaders(logContext);
        return default;
    }

    private static void AddHeaders(HttpLoggingInterceptorContext logContext)
    {
        foreach (var header in logContext.HttpContext.Response.Headers)
        {
            logContext.AddParameter(header.Key, header.Value);
        }
    }
}

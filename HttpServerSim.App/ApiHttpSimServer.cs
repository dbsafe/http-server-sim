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
    private readonly AppConfig _appConfig;
    private readonly RulesConfig rulesConfig;

    private readonly HttpSimRuleStore _ruleStore = new();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiHttpSimServer(string[] args)
    {
        _httpSimRuleResolver = new HttpSimRuleResolver(_ruleStore);
        _httpSimApp = BuildHttpSimApplication(args);

        _appConfig = LoadAppConfig(_httpSimApp.Configuration);
        rulesConfig = LoadRulesConfig(_appConfig);

        _httpSimApp.UseRulesConfig(rulesConfig.Rules, Path.GetDirectoryName(_appConfig.RulesPath)!, _ruleStore);
        _httpSimApp.Urls.Add(_appConfig.Url!);

        // Start control endpoint only when the url is present
        if (!string.IsNullOrEmpty(_appConfig.ControlUrl))
        {
            _controlApp = BuildControlApplication(args);
            _controlApp.Urls.Add(_appConfig.ControlUrl!);
        }
    }

    private WebApplication BuildHttpSimApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();

        AddCustomColorFormatter(builder, false);

        var app = builder.Build();
        app.UseHttpLogging();
        app.UseHttpSimRuleResolver(_httpSimRuleResolver, app.Logger);
        return app;
    }

    private WebApplication BuildControlApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();

        if (_appConfig.LogControlRequestAndResponse) AddCustomColorFormatter(builder, true); else builder.Logging.AddConsole();

        var app = builder.Build();
        if (_appConfig.LogControlRequestAndResponse) app.UseHttpLogging();
        app.MapControlEndpoints(_ruleStore, builder.Environment.ContentRootPath, app.Logger);
        return app;
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

﻿// Ignore Spelling: Api app

using HttpServerSim.App.Config;
using HttpServerSim.App.Contracts;
using HttpServerSim.App.Middleware;
using HttpServerSim.App.Rules;
using HttpServerSim.Contracts;
using HttpServerSim.Models;
using Microsoft.AspNetCore.HttpLogging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerSim;

public sealed class ApiHttpSimServer : IDisposable
{
    private WebApplication? _httpSimApp;
    private WebApplication? _controlApp;
    private readonly IHttpSimRuleResolver _httpSimRuleResolver;

    private readonly HttpSimRuleStore _ruleStore = new();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiHttpSimServer(string[] args, AppConfig appConfig)
    {
        _httpSimRuleResolver = new App.Rules.HttpSimRuleResolver(_ruleStore);

        _httpSimApp = BuildHttpSimApplication(args, isControlEndpoint: false, useHttpLogging: false, appConfig.RequestBodyLogLimit, appConfig.ResponseBodyLogLimit);
        List<IRequestResponseLoggerPresentation> requestResponseLoggerPresentations =
        [
            new ConsoleRequestResponseLoggerPresentation()
        ];

        var _requestResponseLogger = new RequestResponseLogger(_httpSimApp.Logger, appConfig, requestResponseLoggerPresentations);
        if (appConfig.LogRequestAndResponse)
        {
            _httpSimApp.UseRequestResponseLogger(_requestResponseLogger);
        }

        var defaultResponse = BuildDefaultResponse(appConfig, _httpSimApp.Logger);
        _httpSimApp.UseHttpSimRuleResolver(_httpSimRuleResolver, _httpSimApp.Logger, appConfig.ResponseFilesFolder!, defaultResponse);

        var ruleLoaded = TryLoadRulesConfig(appConfig, _httpSimApp.Logger, out RulesConfig? rulesConfig);
        if (ruleLoaded)
        {
            _httpSimApp.UseRulesConfig(rulesConfig!.Rules, appConfig.ResponseFilesFolder!, _ruleStore);
        }

        _httpSimApp.Urls.Add(appConfig.Url!);

        // Start control endpoint only when the url is present
        if (!string.IsNullOrEmpty(appConfig.ControlUrl))
        {
            _controlApp = BuildHttpSimApplication(args, isControlEndpoint: true, useHttpLogging: appConfig.LogControlRequestAndResponse, appConfig.RequestBodyLogLimit, appConfig.ResponseBodyLogLimit);
            _controlApp.MapControlEndpoints(_ruleStore, appConfig.ResponseFilesFolder!, _controlApp.Logger);
            _controlApp.Urls.Add(appConfig.ControlUrl!);
        }
    }


    private static HttpSimResponse BuildDefaultResponse(AppConfig appConfig, ILogger logger)
    {
        var configRule = new ConfigRule
        {
            Name = "default-response-builder",
            Response = new HttpSimResponse
            {
                ContentType = appConfig.DefaultContentType,
                ContentValue = appConfig.DefaultContentValue,
                ContentValueType = ContentValueType.Text,
                StatusCode = appConfig.DefaultStatusCode
            }
        };

        var defaultResponse = RulesConfigHelper.BuildResponseFromRule(logger, configRule, string.Empty) ?? throw new InvalidOperationException($"{nameof(HttpSimResponse)} should not be null here.");

        logger.LogDebug($"Default Response{Environment.NewLine}{JsonSerializer.Serialize(defaultResponse)}");
        return defaultResponse;
    }

    private static WebApplication BuildHttpSimApplication(string[] args, bool isControlEndpoint, bool useHttpLogging, int requestBodyLogLimit, int responseBodyLogLimit)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureHttpJsonOptionsForControlEndpoint(builder, isControlEndpoint);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        ConfigureHttpLogging(builder, useHttpLogging, requestBodyLogLimit, responseBodyLogLimit);

        var app = builder.Build();
        if (useHttpLogging)
        {
            app.UseHttpLogging();
        }

        return app;
    }

    private static void ConfigureHttpLogging(WebApplicationBuilder builder, bool useHttpLogging, int requestBodyLogLimit, int responseBodyLogLimit)
    {
        if (useHttpLogging)
        {
            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All & ~(HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders);
                //logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestBodyLogLimit = requestBodyLogLimit;
                logging.ResponseBodyLogLimit = responseBodyLogLimit;
                //logging.CombineLogs = true;
            });
        }
    }

    private static void ConfigureHttpJsonOptionsForControlEndpoint(WebApplicationBuilder builder, bool isControlEndpoint)
    {
        if (isControlEndpoint)
        {
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.WriteIndented = true;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        }
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

    private static bool TryLoadRulesConfig(AppConfig appConfig, ILogger logger, [NotNullWhen(true)] out RulesConfig? rulesConfig)
    {
        if (appConfig.Rules is null)
        {
            logger.LogWarning("A rules file has not been loaded into the simulator.");
            rulesConfig = null;
            return false;
        }

        string BuildErrorMessage() => $"Failed to load rules from '{appConfig.Rules}.";

        try
        {
            using var fileStream = File.OpenRead(appConfig.Rules);
            rulesConfig = JsonSerializer.Deserialize<RulesConfig>(fileStream, _jsonSerializerOptions);
        }
        catch (Exception ex) when (ex is InvalidDataException || ex is JsonException)
        {
            var error = appConfig.IsDebugMode ? ex.ToString() : ex.Message;
            logger.LogError($"{BuildErrorMessage()}.{Environment.NewLine}{error}");
            rulesConfig = null;
            return false;
        }

        if (rulesConfig == null)
        {
            logger.LogError($"{BuildErrorMessage()}.{Environment.NewLine}Invalid content");
            return false;
        }

        return true;
    }
}

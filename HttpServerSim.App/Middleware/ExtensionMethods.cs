﻿// Ignore Spelling: app

using HttpServerSim.App.Contracts;
using HttpServerSim.App.Models;

namespace HttpServerSim.App.Middleware;

internal static class ExtensionMethods
{
    public static IApplicationBuilder UseHttpSimRuleResolver(this IApplicationBuilder app, IHttpSimRuleResolver httpSimRuleResolver, ILogger logger, string responseFilesFolder, DefaultResponse defaultResponse) =>
        app.Use(HttpSimRuleResolver.CreateMiddlewareHandleRequest(httpSimRuleResolver, logger, responseFilesFolder, defaultResponse));

    public static IApplicationBuilder UseRequestResponseLogger(this IApplicationBuilder app, IRequestResponseLogger requestResponseLogger) =>
        app.Use(HttpSimRuleResolver.CreateMiddlewareRequestResponseLogger(requestResponseLogger));

    public static IEndpointRouteBuilder MapControlEndpoints(this IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, string responseFilesFolder, ILogger logger) =>
        ControlEndpoint.Map(app, ruleStore, responseFilesFolder, logger);
}

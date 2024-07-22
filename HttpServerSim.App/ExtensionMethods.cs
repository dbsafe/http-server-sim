// Ignore Spelling: app

using HttpServerSim.Contracts;

namespace HttpServerSim;

internal static class ExtensionMethods
{
    public static IApplicationBuilder UseHttpSimRuleResolver(this IApplicationBuilder app, IHttpSimRuleResolver httpSimRuleResolver, ILogger logger, string responseFilesFolder) => 
        app.Use(HttpSimRuleResolverHelper.CreateMiddlewareHandleRequest(httpSimRuleResolver, logger, responseFilesFolder));

    public static IEndpointRouteBuilder MapControlEndpoints(this IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, string responseFilesFolder, ILogger logger) => 
        ControlEndpointHelper.Map(app, ruleStore, responseFilesFolder, logger);
}

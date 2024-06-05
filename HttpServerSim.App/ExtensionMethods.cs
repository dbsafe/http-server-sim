// Ignore Spelling: app

using HttpServerSim.Contracts;

namespace HttpServerSim;

internal static class ExtensionMethods
{
    public static IApplicationBuilder UseHttpSimRuleResolver(this IApplicationBuilder app, IHttpSimRuleResolver httpSimRuleResolver, ILogger logger) => 
        app.Use(HttpSimRuleResolverHelper.CreateMiddlewareHandleRequest(httpSimRuleResolver, logger));

    public static IEndpointRouteBuilder MapControlEndpoints(this IEndpointRouteBuilder app, IHttpSimRuleStore ruleStore, string contentRoot, ILogger logger) => 
        ControlEndpointHelper.Map(app, ruleStore, contentRoot, logger);
}

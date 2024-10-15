// Ignore Spelling: app

namespace HttpServerSim.App.Middleware;

// Consolidate all the logic for Swagger in one place to make a transition in the future easier.
// Swagger will be removed in .NET 9.
public static class SwaggerHelper
{
    public static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void ConfigureSwagger(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });
    }
}

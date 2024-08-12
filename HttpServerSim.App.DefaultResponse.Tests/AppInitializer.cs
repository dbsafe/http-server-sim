// Ignore Spelling: App

#nullable disable

using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.DefaultResponse.Tests;

[TestClass]
public static class AppInitializer
{
    public static HttpServerSimHostTest TestHost { get; private set; }

    [AssemblyInitialize]
    public static void StartApp(TestContext testContext)
    {
        var args = "--Rules rules.json --DefaultContentValue moved --DefaultContentType text/plain --DefaultStatusCode 301 --Logging:LogLevel:HttpServerSim Debug";
        TestHost = new HttpServerSimHostTest(testContext, args);
        TestHost.Start();
    }

    [AssemblyCleanup]
    public static void StopApp()
    {
        TestHost?.Stop();
        TestHost?.Dispose();
    }
}

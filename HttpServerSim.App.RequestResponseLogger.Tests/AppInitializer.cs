// Ignore Spelling: App

using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.RequestResponseLogger.Tests;

[TestClass]
public static class AppInitializer
{
    public const string TEST_SIM_HOST = "localhost:5002";
    public const string TEST_SIM_URL = $"http://{TEST_SIM_HOST}";

    public static HttpServerSimHostTest TestHost { get; private set; }
    
    public static void Lock()
    {
        Monitor.Enter(TestHost);
    }

    public static void Unlock()
    {
        Monitor.Exit(TestHost);
    }

    [AssemblyInitialize]
    public static void StartApp(TestContext testContext)
    {
        var args = $"--Url {TEST_SIM_URL} --Rules rules.json --RequestBodyLogLimit 51 --ResponseBodyLogLimit 52 --SaveRequests http-server-sim-history --SaveResponses http-server-sim-history --Logging:LogLevel:HttpServerSim Debug";
        TestHost = new HttpServerSimHostTest(testContext, args, TEST_SIM_URL);
        TestHost.Start();
    }

    [AssemblyCleanup]
    public static void StopApp()
    {
        TestHost?.Stop();
        TestHost?.FlushLogs();
        TestHost?.Dispose();
    }
}

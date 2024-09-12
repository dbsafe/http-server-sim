// Ignore Spelling: App

using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.StaticRules.Tests;

[TestClass]
public static class AppInitializer
{
    public const string TEST_SIM_URL = "http://localhost:5002";
    public const string TEST_SIM_CONTROL_URL = "http://localhost:5003";

    private static readonly object _locker = new();
    public static HttpServerSimHostTest TestHost { get; private set; }

    public static void Lock()
    {
        Monitor.Enter(_locker);
    }

    public static void Unlock()
    {
        Monitor.Exit(_locker);
    }

    [AssemblyInitialize]
    public static void StartApp(TestContext testContext)
    {
        var args = $"--Rules test-rules.json --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Url {TEST_SIM_URL} --ControlUrl {TEST_SIM_CONTROL_URL}";
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

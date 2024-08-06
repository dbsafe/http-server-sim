// Ignore Spelling: App

using HttpServerSim.App.Tests.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace HttpServerSim.App.RequestResponseLogger.Tests;

[TestClass]
public static class AppInitializer
{
    private static HttpServerSimHost? _testHost;

    public static readonly HttpClient HttpClient = HttpServerSimHost.HttpClient;
    public static string SimulatorUrl { get; } = "http://localhost:5000";
    public static string? HistoryFolder { get; private set; }

#pragma warning disable IDE0052 // Remove unread private members
    private static TestContext? _testContext;
#pragma warning restore IDE0052 // Remove unread private members

    [AssemblyInitialize]
    public static void StartApp(TestContext testContext)
    {
        _testContext = testContext;
        
        var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Executable not found");
        var relativePath = "../../../../HttpServerSim.App";
        var projectDirectory = Path.GetFullPath(relativePath, testDirectory);
        HistoryFolder = Path.Combine(projectDirectory, "http-server-sim-history");

        // `dotnet run` (without args) uses the args used in launchSettings.json
        _testHost = new HttpServerSimHost(SimulatorUrl, projectDirectory, "dotnet", $"run --Rules rules.json --RequestBodyLogLimit 51 --ResponseBodyLogLimit 52 --SaveRequests http-server-sim-history --SaveResponses http-server-sim-history");
        _testHost.Start();
    }

    public static bool TryFindSection(string startToken, string endToken, StringBuilder logs, [NotNullWhen(true)] out string? section) =>
        _testHost!.TryFindSection(startToken, endToken, logs, out section);

    [AssemblyCleanup]
    public static void StopApp()
    {
        _testHost?.Stop();
        _testHost?.Dispose();
    }

    public static void FlushLogs()
    {
        ArgumentNullException.ThrowIfNull(_testHost);
        while (_testHost.LogsQueue.TryDequeue(out var log))
        {
            Console.WriteLine(log);
        }
    }
}

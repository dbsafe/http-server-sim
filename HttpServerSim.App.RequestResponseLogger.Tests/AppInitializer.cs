// Ignore Spelling: App

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace HttpServerSim.App.RequestResponseLogger.Tests
{
    [TestClass]
    public static class AppInitializer
    {
        public static readonly HttpClient HttpClient = new();
        public static string SimulatorUrl { get; } = "http://localhost:5000";

        private static ConsoleAppRunner? _consoleAppRunner;
        private static TestContext? _testContext;
        public static ConcurrentQueue<string> LogsQueue { get; } = new();

        [AssemblyInitialize]
        public static void StartApp(TestContext testContext)
        {
            Assert.IsFalse(SendSimpleRequest(), "There is a process already listening in the same port");

            _testContext = testContext;

            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Executable not found");
            var relativePath = "../../../../HttpServerSim.App";
            var projectDirectory = Path.GetFullPath(relativePath, testDirectory);

            // `dotnet run` (without args) uses the args used in launchSettings.json
            _consoleAppRunner = new ConsoleAppRunner(projectDirectory, "dotnet", $"run --Rules rules.json --RequestBodyLogLimit 51 --ResponseBodyLogLimit 52");
            _consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
            _consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;

            _consoleAppRunner.Start();

            Assert.IsTrue(WaitForServiceUsingARequest(_consoleAppRunner, TimeSpan.FromSeconds(10), _testContext), "Service was not ready");
        }

        public static bool TryFindSection(string startToken, string endToken, StringBuilder logs, TimeSpan timeout, [NotNullWhen(true)] out string? section)
        {
            var expiration = DateTimeOffset.UtcNow + timeout;
            while (expiration > DateTimeOffset.UtcNow)
            {
                while (LogsQueue.TryDequeue(out var log))
                {
                    Console.WriteLine(log);
                    logs.AppendLine(log);
                }

                var currentLogs = logs.ToString();
                var startIndex = currentLogs.IndexOf(startToken);
                var endIndex = currentLogs.IndexOf(endToken);

                if (startIndex == -1 || endIndex == -1)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (startIndex > endIndex)
                {
                    section = null;
                    return false;
                }

                section = currentLogs.Substring(startIndex, endIndex - startIndex + endToken.Length);
                return true;
            }

            section = null;
            return false;
        }

        private static bool WaitForResponse()
        {
            var sb = new StringBuilder();
            return TryFindSection("Response:", "End of Response", sb, TimeSpan.FromSeconds(5), out _);
        }

        private static bool SendSimpleRequest()
        {
            try
            {
                HttpClient.GetAsync($"{SimulatorUrl}/confirming-initialization").Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool WaitForServiceUsingARequest(ConsoleAppRunner consoleAppRunner, TimeSpan timeout, TestContext testContext)
        {
            void OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                // This is used for debugging only because the same logs are logged from the test.
                // testContext.WriteLine(e.Data);
            }

            consoleAppRunner.OutputDataReceived += OutputDataReceived;
            try
            {
                var expiration = DateTimeOffset.UtcNow + timeout;
                while (expiration > DateTimeOffset.UtcNow)
                {
                    // Attempt to send request multiple times because the App is initializing
                    if (SendSimpleRequest())
                    {
                        // Attempt to get the response only one time because the app is already initialized
                        return WaitForResponse();
                    }

                    Thread.Sleep(100);
                }

                return false;
            }
            finally
            {
                consoleAppRunner.OutputDataReceived -= OutputDataReceived;
            }
        }

        [AssemblyCleanup]
        public static void StopApp()
        {
            _consoleAppRunner?.Stop();
            _consoleAppRunner?.Dispose();
        }

        private static void ConsoleAppRunner_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
            {
                LogsQueue.Enqueue(e.Data);
            }
        }
    }
}

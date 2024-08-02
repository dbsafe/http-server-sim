// Ignore Spelling: App

using System.Collections.Concurrent;
using System.Diagnostics;
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

            _consoleAppRunner = new ConsoleAppRunner(projectDirectory, "dotnet", $"run");
            _consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
            _consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;

            _consoleAppRunner.Start();

            Assert.IsTrue(WaitForServiceUsingARequest(_consoleAppRunner, TimeSpan.FromSeconds(10), _testContext), "Service was not ready");
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
                    if (SendSimpleRequest())
                    {
                        return true;
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

        private static bool WaitForServiceUsingLogs(ConsoleAppRunner consoleAppRunner, TimeSpan timeout, TestContext testContext)
        {
            var logs = new StringBuilder();
            long foundCount = 0;

            void OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                logs.AppendLine(e.Data);
                var currentLogs = logs.ToString();
                Console.WriteLine($"Current Logs: {currentLogs}");
                
                if (currentLogs.Contains("Now listening on: http://localhost:5000"))
                {
                    Interlocked.Increment(ref foundCount);
                }
            }

            consoleAppRunner.OutputDataReceived += OutputDataReceived;
            try
            {
                var expiration = DateTimeOffset.UtcNow + timeout;
                while (expiration > DateTimeOffset.UtcNow)
                {
                    if (Interlocked.Read(ref foundCount) > 0)
                    {
                        return true;
                    }

                    Thread.Sleep(100);
                }
            }
            finally
            {
                consoleAppRunner.OutputDataReceived -= OutputDataReceived;
            }

            return false;
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

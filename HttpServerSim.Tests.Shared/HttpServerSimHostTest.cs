using HttpServerSim.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerSim.Tests.Shared
{
    public class HttpServerSimHostTest : IDisposable
    {
        private static readonly TimeSpan _defaulTimeout = TimeSpan.FromSeconds(10);
        private readonly HttpServerSimHost _testHost;
        private readonly ConcurrentQueue<string> _logsQueue = new();
        public readonly HttpClient HttpClient = HttpServerSimHost.HttpClient;
        private static TestContext _testContext;

        // TODO: Remove this property. It should be passed in the constructor
        public string SimulatorUrl { get; } = "http://localhost:5000";

        private bool _disposed = false;

        public HttpServerSimHostTest(TestContext testContext, string args, string simulatorUrl = null)
        {
            SimulatorUrl = simulatorUrl ?? SimulatorUrl;

            _testContext = testContext;
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Executable not found");

            // `dotnet run` (without args) uses the args from launchSettings.json
            _testHost = new HttpServerSimHost(SimulatorUrl, testDirectory, "dotnet", $"HttpServerSim.App.dll {args}");
            _testHost.LogReceived += TestHost_LogReceived;
        }

        public void Start(bool waitForServiceToBeReady = true)
        {
            _testHost.Start();

            if (waitForServiceToBeReady && !WaitForLog($"Now listening on: {SimulatorUrl}"))
            {
                // Need to stop the host from here because the test may not have the chance to do it
                _testHost.Stop();
                throw new InvalidOperationException("Service was not ready");
            }
        }

        private bool WaitForLog(string token, TimeSpan? timeout = null)
        {
            timeout ??= _defaulTimeout;
            _logsQueue.Enqueue($"Waiting for log '{token}' for {timeout.Value.TotalMilliseconds} Milliseconds");
            var task = TryFindLog(token, timeout.Value);

            return task.Wait(timeout.Value) && task.Result;
        }

        private Task<bool> TryFindLog(string token, TimeSpan timeout)
        {
            return Task.Run(() =>
            {
                var logs = new StringBuilder();
                var expiration = DateTimeOffset.UtcNow + timeout;

                void LogReceived(object sender, ConsoleAppRunnerEventArgs e)
                {
                    lock (logs)
                    {
                        logs.AppendLine(e.Data);
                    }
                }

                _testHost.LogReceived += LogReceived;
                try
                {
                    while (expiration > DateTimeOffset.UtcNow)
                    {
                        Thread.Sleep(10);
                        string currentLogs;
                        lock (logs)
                        {
                            currentLogs = logs.ToString();
                        }

                        if (currentLogs.Contains(token))
                        {
                            return true;
                        }
                    }
                }
                finally
                {
                    _testHost.LogReceived -= LogReceived;
                }

                return false;
            });
        }

        public Task<string> TryFindLogSection(string startToken, string endToken, TimeSpan? timeout = null)
        {
            timeout = timeout ?? _defaulTimeout;
            return Task.Run(() =>
            {
                var logs = new StringBuilder();
                var expiration = DateTimeOffset.UtcNow + timeout;
                
                void LogReceived(object sender, ConsoleAppRunnerEventArgs e)
                {
                    lock (logs)
                    {
                        logs.AppendLine(e.Data);
                    }
                }

                _testHost.LogReceived += LogReceived;
                try
                {
                    while (expiration > DateTimeOffset.UtcNow)
                    {
                        Thread.Sleep(10);
                        string currentLogs;
                        lock (logs)
                        {
                            currentLogs = logs.ToString();
                        }

                        var startIndex = currentLogs.IndexOf(startToken);
                        var endIndex = currentLogs.IndexOf(endToken);

                        if (startIndex == -1 || endIndex == -1)
                        {
                            continue;
                        }

                        if (startIndex > endIndex)
                        {
                            return null;
                        }

                        return currentLogs.Substring(startIndex, endIndex - startIndex + endToken.Length);
                    }

                    return null;
                }
                finally
                {
                    _testHost.LogReceived -= LogReceived;
                }
            });
        }

        public void Stop()
        {
            _testHost.Stop();
        }

        public bool RunAndWaitForExit(TimeSpan timeout) => _testHost.RunAndWaitForExit(timeout);

        private void TestHost_LogReceived(object sender, ConsoleAppRunnerEventArgs e) => _logsQueue.Enqueue(e.Data);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _testHost.Dispose();
                }

                _disposed = true;
            }
        }

        ~HttpServerSimHostTest() => Dispose(disposing: false);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public string FlushLogs()
        {
            var sb = new StringBuilder();
            while (_logsQueue.TryDequeue(out var log))
            {
                sb.AppendLine(log);
            }

            var logs = sb.ToString();
            if (!string.IsNullOrEmpty(logs))
            {
                _testContext.WriteLine($"[HttpServerSim.App]{Environment.NewLine}{logs}");
            }

            return logs;
        }
    }
}

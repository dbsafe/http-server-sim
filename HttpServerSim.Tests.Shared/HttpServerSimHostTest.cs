using HttpServerSim.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace HttpServerSim.Tests.Shared
{
    public class HttpServerSimHostTest : IDisposable
    {
        private readonly HttpServerSimHost _testHost;
        private readonly ConcurrentQueue<string> _logsQueue = new();
        public readonly HttpClient HttpClient = HttpServerSimHost.HttpClient;
        private static TestContext _testContext;

        public string SimulatorUrl { get; } = "http://localhost:5000";

        private bool _disposed = false;

        public HttpServerSimHostTest(TestContext testContext, string args)
        {
            _testContext = testContext;
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Executable not found");

            // `dotnet run` (without args) uses the args from launchSettings.json
            _testHost = new HttpServerSimHost(SimulatorUrl, testDirectory, "dotnet", $"HttpServerSim.App.dll {args}");
            _testHost.LogReceived += TestHost_LogReceived;
        }

        public void Start() => _testHost.Start();

        public void Stop()
        {
            FlushLogs();
            _testHost?.Stop();
            _testHost?.Dispose();
        }

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

        public bool TryFindSection(string startToken, string endToken, out string section) =>
            _testHost!.TryFindSection(startToken, endToken, out section);

        public void FlushLogs()
        {
            var sb = new StringBuilder();
            while (_logsQueue.TryDequeue(out var log))
            {
                sb.AppendLine(log);
            }

            if (sb.Length > 0)
            {
                _testContext.WriteLine($"[HttpServerSim.App]{Environment.NewLine}{sb}");
            }
        }
    }
}

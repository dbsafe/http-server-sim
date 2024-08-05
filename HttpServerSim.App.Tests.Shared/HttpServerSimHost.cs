// Ignore Spelling: App

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HttpServerSim.App.Tests.Shared;

public class HttpServerSimHost : IDisposable
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
    private readonly ConsoleAppRunner _consoleAppRunner;
    private readonly string _simulatorUrl;
    private bool _disposed = false;

    public static readonly HttpClient HttpClient = new();        
    public ConcurrentQueue<string> LogsQueue { get; } = new();

    public HttpServerSimHost(string simulatorUrl, string projectDirectory, string filenameOrCommand, string args)
    {
        _simulatorUrl = simulatorUrl;
        _consoleAppRunner = new ConsoleAppRunner(projectDirectory, filenameOrCommand, args);
        _consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
        _consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _consoleAppRunner.Dispose();
            }

            _disposed = true;
        }
    }

    ~HttpServerSimHost() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public bool SendSimpleRequest()
    {
        try
        {
            HttpClient.GetAsync($"{_simulatorUrl}/confirming-initialization").Wait();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ConsoleAppRunner_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
        {
            LogsQueue.Enqueue(e.Data);
        }
    }

    public void Start()
    {
        Console.WriteLine($"{nameof(HttpServerSimHost)} - Starting ...");
        Assert.IsFalse(SendSimpleRequest(), "There is a process already listening in the same port");
        _consoleAppRunner.Start();
        Assert.IsTrue(WaitForServiceUsingARequest(), "Service was not ready");
    }

    public void Stop()
    {
        Console.WriteLine($"{nameof(HttpServerSimHost)} - Stoping ...");
        _consoleAppRunner.Stop();
    }

    private bool WaitForServiceUsingARequest()
    {
        static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // This is used for debugging only because the same logs are logged from the test.
            // Requires adding TestContext testContext to the constructor
            // testContext.WriteLine(e.Data);
        }

        _consoleAppRunner.OutputDataReceived += OutputDataReceived;
        try
        {
            var expiration = DateTimeOffset.UtcNow + _timeout;
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
            _consoleAppRunner.OutputDataReceived -= OutputDataReceived;
        }
    }

    private bool WaitForResponse()
    {
        var sb = new StringBuilder();
        return TryFindSection("Response:", "End of Response", sb, out _);
    }

    public bool TryFindSection(string startToken, string endToken, StringBuilder logs, [NotNullWhen(true)] out string? section)
    {
        var expiration = DateTimeOffset.UtcNow + _timeout;
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
}

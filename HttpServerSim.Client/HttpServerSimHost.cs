// Ignore Spelling: App

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace HttpServerSim.Client;

public class HttpServerSimHost : IDisposable
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
    private readonly ConsoleAppRunner _consoleAppRunner;
    private readonly string _simulatorUrl;
    private bool _disposed = false;

    public event EventHandler<ConsoleAppRunnerEventArgs>? LogReceived;

    public static readonly HttpClient HttpClient = new();        
    private readonly ConcurrentQueue<string> _logsQueue = new();

    public HttpServerSimHost(string simulatorUrl, string workingDirectory, string filenameOrCommand, string args)
    {
        _simulatorUrl = simulatorUrl;
        _consoleAppRunner = new ConsoleAppRunner(workingDirectory, filenameOrCommand, args);
        _consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
        _consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;
    }

    protected virtual void OnLogReceived(ConsoleAppRunnerEventArgs e) => LogReceived?.Invoke(this, e);

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

    private void ConsoleAppRunner_OutputDataReceived(object sender, ConsoleAppRunnerEventArgs e)
    {
        if (e.Data is not null)
        {
            _logsQueue.Enqueue(e.Data);
        }
    }

    public void Start()
    {
        _logsQueue.Enqueue($"{nameof(HttpServerSimHost)} - Starting ...");

        if (SendSimpleRequest())
        {
            throw new InvalidOperationException("There is a process already listening in the same port");
        }
        
        _consoleAppRunner.Start();
        if (!WaitForServiceUsingARequest())
        {
            throw new InvalidOperationException("Service was not ready");
        }
    }

    public void Stop()
    {
        _logsQueue.Enqueue($"{nameof(HttpServerSimHost)} - Stopping ...");
        FlushLogs();
        _consoleAppRunner.Stop();
    }

    public bool RunAndWaitForExit(TimeSpan timeout) 
    {
        _logsQueue.Enqueue($"{nameof(HttpServerSimHost)} - Starting process and waiting ...");
        var stopped =_consoleAppRunner.RunAndWaitForExit(timeout);
        if (stopped)
        {
            _logsQueue.Enqueue($"{nameof(HttpServerSimHost)} - Stopped");
        }
        else
        {
            _logsQueue.Enqueue($"{nameof(HttpServerSimHost)} - Failed to stop");
        }

        FlushLogs();
        return stopped;
    }
    
    private bool WaitForServiceUsingARequest()
    {
        static void OutputDataReceived(object sender, ConsoleAppRunnerEventArgs e)
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

    private bool WaitForResponse() => TryFindSection("Response:", "End of Response", out _);

    private string FlushLogs()
    {
        var logs = new StringBuilder();
        while (_logsQueue.TryDequeue(out var log))
        {
            logs.Append(log);
            OnLogReceived(new ConsoleAppRunnerEventArgs(log));
        }

        return logs.ToString();
    }

    public bool TryFindSection(string startToken, string endToken, out string? section)
    {
        var logs = new StringBuilder();
        var expiration = DateTimeOffset.UtcNow + _timeout;
        while (expiration > DateTimeOffset.UtcNow)
        {
            Thread.Sleep(10);

            if (_logsQueue.TryDequeue(out var log))
            {
                logs.AppendLine(log);
                OnLogReceived(new ConsoleAppRunnerEventArgs(log));
            }

            var currentLogs = logs.ToString();
            var startIndex = currentLogs.IndexOf(startToken);
            var endIndex = currentLogs.IndexOf(endToken);

            if (startIndex == -1 || endIndex == -1)
            {
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

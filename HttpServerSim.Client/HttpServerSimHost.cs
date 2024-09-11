// Ignore Spelling: App

using System;
using System.Net.Http;

namespace HttpServerSim.Client;

public class HttpServerSimHost : IDisposable
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
    private readonly ConsoleAppRunner _consoleAppRunner;
    private readonly string _simulatorUrl;
    private bool _disposed = false;

    public event EventHandler<ConsoleAppRunnerEventArgs>? LogReceived;

    public static readonly HttpClient HttpClient = new();        

    public HttpServerSimHost(string simulatorUrl, string workingDirectory, string filenameOrCommand, string args)
    {
        _simulatorUrl = simulatorUrl;
        _consoleAppRunner = new ConsoleAppRunner(workingDirectory, filenameOrCommand, args);
        _consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
        _consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;
    }

    protected virtual void OnLogReceived(ConsoleAppRunnerEventArgs e) => LogReceived?.Invoke(this, e);
    private void Log(string data) => LogReceived?.Invoke(this, new ConsoleAppRunnerEventArgs($"{nameof(HttpServerSimHost)} - {data}"));

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
        OnLogReceived(e);
    }

    public void Start()
    {
        Log("Starting ...");

        if (SendSimpleRequest())
        {
            throw new InvalidOperationException("There is a process already listening in the same port");
        }
        
        _consoleAppRunner.Start();
    }

    public void Stop()
    {
        Log("Stopping ...");
        _consoleAppRunner.Stop();
    }

    public bool RunAndWaitForExit(TimeSpan timeout) 
    {
        Log("Starting process and waiting ...");
        
        var stopped =_consoleAppRunner.RunAndWaitForExit(timeout);
        if (stopped)
        {
            Log("Stopped");
        }
        else
        {
            Log("Failed to stop");
        }

        return stopped;
    }
}

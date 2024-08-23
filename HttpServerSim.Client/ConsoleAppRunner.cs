// Ignore Spelling: App

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HttpServerSim.Client;

public class ConsoleAppRunnerEventArgs(string? data) : EventArgs
{
    public string? Data { get; } = data;
}

public class ConsoleAppRunner : IDisposable
{
    private readonly Process _process;
    private bool _disposed = false;
    private bool _isProcessRunning = false;

    public int ExitCode => _process.ExitCode;

    public event EventHandler<ConsoleAppRunnerEventArgs>? OutputDataReceived;
    public event EventHandler<ConsoleAppRunnerEventArgs>? ErrorDataReceived;

    public ConsoleAppRunner(string workingDirectory, string filenameOrCommand, string args)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = workingDirectory,
                FileName = filenameOrCommand,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
            OnOutputDataReceived(sender, new ConsoleAppRunnerEventArgs(e.Data));
        };

        _process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
            OnErrorDataReceived(sender, new ConsoleAppRunnerEventArgs(e.Data));
        };
    }

    ~ConsoleAppRunner() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _process.Dispose();
            }

            _disposed = true;
        }
    }

    public void Start()
    {
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        _isProcessRunning = true;
    }

    public void Stop()
    {
        if (!_isProcessRunning)
        {
            return;
        }
        _process.Kill();
        _process.WaitForExit();
        _isProcessRunning = false;
    }

    public bool RunAndWaitForExit(TimeSpan timeout)
    {
        Start();
        var stopped = _process.WaitForExit((int)timeout.TotalMilliseconds);
        Stop();
        return stopped;
    }

    private void OnOutputDataReceived(object sender, ConsoleAppRunnerEventArgs e) => OutputDataReceived?.Invoke(sender, e);
    private void OnErrorDataReceived(object sender, ConsoleAppRunnerEventArgs e) => ErrorDataReceived?.Invoke(sender, e);
}

// Keeping this class as a reference of using Process output different
internal class ConsoleAppRunner2 : IDisposable
{
    private readonly Process _process;
    private bool _disposed = false;
    private bool _isProcessRunning = false;

    public int ExitCode => _process.ExitCode;

    public event EventHandler<ConsoleAppRunnerEventArgs>? OutputDataReceived;
    public event EventHandler<ConsoleAppRunnerEventArgs>? ErrorDataReceived;

    public ConsoleAppRunner2(string workingDirectory, string filenameOrCommand, string args)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = workingDirectory,
                FileName = filenameOrCommand,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    ~ConsoleAppRunner2() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _process.Dispose();
            }

            _disposed = true;
        }
    }

    public void Start()
    {
        _process.Start();

        Task.Run(async () =>
        {
            string text;

            while ((text = await _process.StandardOutput.ReadLineAsync()) != null)
            {
                OnOutputDataReceived(this, new ConsoleAppRunnerEventArgs(text));
            }
        });

        Task.Run(async () =>
        {
            string text;
            while ((text = await _process.StandardError.ReadLineAsync()) != null)
            {
                OnErrorDataReceived(this, new ConsoleAppRunnerEventArgs(text));
            }
        });
        _isProcessRunning = true;
    }

    public void Stop()
    {
        if (!_isProcessRunning)
        {
            return;
        }

        _process.Kill();
        _process.WaitForExit();
    }

    public bool RunAndWaitForExit(TimeSpan timeout)
    {
        Start();
        return _process.WaitForExit((int)timeout.TotalMilliseconds);
    }

    private void OnOutputDataReceived(object sender, ConsoleAppRunnerEventArgs e) => OutputDataReceived?.Invoke(sender, e);
    private void OnErrorDataReceived(object sender, ConsoleAppRunnerEventArgs e) => ErrorDataReceived?.Invoke(sender, e);
}

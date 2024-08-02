// Ignore Spelling: App

using System.Diagnostics;

namespace HttpServerSim.App.RequestResponseLogger.Tests
{
    public class LogEventArgs(string log) : EventArgs
    {
        public string Log { get; } = log;
    }

    public class ConsoleAppRunner : IDisposable
    {
        private Process _process;
        private bool _disposed = false;

        public event DataReceivedEventHandler OutputDataReceived
        {
            add => _process.OutputDataReceived += value;
            remove => _process.OutputDataReceived -= value;
        }

        public event DataReceivedEventHandler ErrorDataReceived
        {
            add => _process.ErrorDataReceived += value;
            remove => _process.ErrorDataReceived -= value;
        }

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
        }

        public void Stop()
        {
            if (!_process.HasExited)
            {
                // Close() sounds like a better option to terminate the process but in a Mac Kill(true) was the only way to stop the app.
                _process.Kill(true);
            }
        }
    }
}

// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.CSharpInteractive
{
    using System;
    using System.Diagnostics;
    using Cmd;
    using Contracts;

    internal class ProcessManager: IProcessManager
    {
        private readonly ILog<ProcessManager> _log;
        private readonly IProcessOutputWriter _processOutputWriter;
        private readonly IStartInfoFactory _startInfoFactory;
        private readonly Text _stdOutPrefix;
        private readonly Text _stdErrPrefix;
        private readonly Process _process;
        private Text _processIdText;
        private int _disposed;
        private IStartInfo? _processInfo;

        public ProcessManager(
            ILog<ProcessManager> log,
            IProcessOutputWriter processOutputWriter,
            IStringService stringService,
            IStartInfoFactory startInfoFactory)
        {
            _log = log;
            _processOutputWriter = processOutputWriter;
            _startInfoFactory = startInfoFactory;
            _stdOutPrefix = new Text($"{stringService.Tab}OUT: "); 
            _stdErrPrefix = new Text($"{stringService.Tab}ERR: ", Color.Error);
            _process = new Process{ EnableRaisingEvents = true };
            _process.OutputDataReceived += ProcessOnOutputDataReceived;
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.Exited += ProcessOnExited;
        }

        public event Action<Output>? OnOutput;

        public event Action? OnExit;

        public int Id { get; private set; }

        public int ExitCode => _process.ExitCode;

        public bool Start(IStartInfo info, out ProcessStartInfo startInfo)
        {
            _processInfo = info;
            startInfo = _process.StartInfo = _startInfoFactory.Create(info);
            if (!_process.Start())
            {
                return false;
            }
            
            try
            {
                 Id = _process.Id;
            }
            catch
            {
                // ignored
            }

            _processIdText = new Text(Id.ToString().PadRight(5));
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            return true;
        }

        public void WaitForExit() => _process.WaitForExit();

        public bool WaitForExit(TimeSpan timeout) => _process.WaitForExit((int)timeout.TotalMilliseconds);

        public void Kill() => _process.Kill();

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e) => ProcessOutput(e, false);

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e) => ProcessOutput(e, true);
            
        private void ProcessOnExited(object? sender, EventArgs e) => OnExit?.Invoke();

        private void ProcessOutput(DataReceivedEventArgs e, bool isError)
        {
            var line = e.Data;
            if (line == default)
            {
                return;
            }

            var handler = OnOutput;
            var output = new Output(_processInfo!, isError, line);
            if (handler != default)
            {
                _log.Trace(() => new []{_processIdText, isError ? _stdErrPrefix : _stdOutPrefix, new Text(line)}, "=>");
                handler(output);
            }
            else
            {
                _processOutputWriter.Write(output);
            }
        }

        public void Dispose()
        {
            try
            {
                if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 0)
                {
                    return;
                }
                
                _process.Exited -= ProcessOnExited;
                _process.OutputDataReceived -= ProcessOnOutputDataReceived;
                _process.ErrorDataReceived -= ProcessOnErrorDataReceived;
                _process.Dispose();
            }
            catch (Exception exception)
            {
                _log.Trace(() => new []{new Text($"Exception during disposing: {exception}.")});
            }
        }
    }
}
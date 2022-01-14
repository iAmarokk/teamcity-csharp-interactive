// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ForCanBeConvertedToForeach
namespace TeamCity.CSharpInteractive
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Cmd;

    internal class ProcessRunner: IProcessRunner
    {
        private readonly Func<IProcessManager> _processManagerFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ProcessRunner(
            Func<IProcessManager> processManagerFactory,
            CancellationTokenSource cancellationTokenSource)
        {
            _processManagerFactory = processManagerFactory;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public ProcessResult Run(IStartInfo startInfo, Action<Output>? handler, IProcessStateProvider? stateProvider, IProcessMonitor monitor, TimeSpan timeout)
        {
            using var processManager = _processManagerFactory();
            if (handler != default)
            {
                processManager.OnOutput += handler;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!processManager.Start(startInfo))
            {
                stopwatch.Stop();
                monitor.Finished(stopwatch.ElapsedMilliseconds, ProcessState.Fail);
                return new ProcessResult(ProcessState.Fail);
            }

            monitor.Started(startInfo, processManager.Id);
            var finished = true;
            if (timeout == TimeSpan.Zero)
            {
                processManager.WaitForExit();
            }
            else
            {
                finished = processManager.WaitForExit(timeout);
            }

            if (finished)
            {
                stopwatch.Stop();
                var state = stateProvider?.GetState(processManager.ExitCode) ?? ProcessState.Unknown;
                monitor.Finished(stopwatch.ElapsedMilliseconds, state, processManager.ExitCode);
                return new ProcessResult(state, processManager.ExitCode);
            }

            processManager.TryKill();
            stopwatch.Stop();
            monitor.Finished(stopwatch.ElapsedMilliseconds, ProcessState.Cancel);

            return new ProcessResult(ProcessState.Cancel);
        }

        public async Task<ProcessResult> RunAsync(IStartInfo startInfo, Action<Output>? handler, IProcessStateProvider? stateProvider, IProcessMonitor monitor, CancellationToken cancellationToken)
        {
            if (cancellationToken == default)
            {
                cancellationToken = _cancellationTokenSource.Token;
            }

            var processManager = _processManagerFactory();
            if (handler != default)
            {
                processManager.OnOutput += handler;
            }
            
            var completionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            // ReSharper disable once AccessToDisposedClosure
            processManager.OnExit += () => completionSource.TrySetResult(processManager.ExitCode);
            var stopwatch = new Stopwatch();
            if (!processManager.Start(startInfo))
            {
                stopwatch.Stop();
                monitor.Finished(stopwatch.ElapsedMilliseconds, ProcessState.Fail);
                processManager.Dispose();
                return new ProcessResult(ProcessState.Fail);
            }
            
            monitor.Started(startInfo, processManager.Id);
            void Cancel()
            {
                if (processManager.TryKill())
                {
                    completionSource.TrySetCanceled(cancellationToken);
                }
                
                processManager.Dispose();
                stopwatch.Stop();
                monitor.Finished(stopwatch.ElapsedMilliseconds, ProcessState.Cancel);
            }

            await using (cancellationToken.Register(Cancel, false))
            {
                using (processManager)
                {
                    var exitCode = await completionSource.Task.ConfigureAwait(false);
                    stopwatch.Start();
                    var state = stateProvider?.GetState(exitCode) ?? ProcessState.Unknown;
                    monitor.Finished(stopwatch.ElapsedMilliseconds, state, exitCode);
                    return new ProcessResult(state, exitCode);
                }
            }
        }
    }
}
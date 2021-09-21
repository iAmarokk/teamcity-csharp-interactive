// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.CSharpInteractive
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class HostService : IHost, IServiceProvider
    {
        private readonly ILog<HostService> _log;
        private readonly ISettings _settings;
        private readonly IStdOut _stdOut;
        
        public HostService(
            ILog<HostService> log,
            ISettings settings,
            IStdOut stdOut)
        {
            _log = log;
            _settings = settings;
            _stdOut = stdOut;
        }

        public IHost Host => this;

        public IReadOnlyList<string> Args => _settings.ScriptArguments;

        public IReadOnlyDictionary<string, string> Props => _settings.ScriptProperties;

        public void WriteLine() => _stdOut.WriteLine();

        public void WriteLine<T>(T line, Color color = Color.Default) => _stdOut.WriteLine(new Text(line?.ToString() ?? string.Empty, color));

        public void Error(string error, string errorId = "Unknown") => _log.Error(new ErrorId(errorId), error);

        public void Warning(string warning) => _log.Warning(warning);

        public void Info(string text) => _log.Info(text);

        public void Trace(string trace, string origin = "") => _log.Trace(origin, trace);

        public object GetService(Type serviceType) => Composer.Resolve(serviceType);

        public T GetService<T>() => Composer.Resolve<T>();
    }
}
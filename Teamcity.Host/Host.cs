namespace Teamcity.Host
{
    using System;

    public static class Host
    {
        private static readonly HostCompositionRoot Root = Composer.Resolve<HostCompositionRoot>();

        public static void ScriptInternal_SetSessionId(string sessionId) =>
            Root.Session.Id = sessionId;

        public static void ScriptInternal_FinishCommand() =>
            Root.SessionObserver.OnNext(new DtoSession { SessionId = Root.Session.Id});
        
        public static void WriteLine() =>
            Root.StdOutObserver.OnNext(new DtoStdOut { Line = Environment.NewLine, Color = Color.Default });

        public static void WriteLine<T>(T line, Color color = Color.Default) =>
            Root.StdOutObserver.OnNext(new DtoStdOut { Line = line?.ToString() ?? string.Empty, Color = color });

        public static void Error(string error, string errorId = "Unknown") =>
            Root.ErrorObserver.OnNext(new DtoError { ErrorId = errorId, Error = error });

        public static void Warning(string warning) =>
            Root.WarningObserver.OnNext(new DtoWarning { Wraning = warning });

        public static void Info(string text) =>
            Root.InfoObserver.OnNext(new DtoInfo { Text = text });

        public static void Trace(string trace) =>
            Root.TraceObserver.OnNext(new DtoTrace { Trace = trace });
    }
}
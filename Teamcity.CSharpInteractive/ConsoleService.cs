// ReSharper disable ClassNeverInstantiated.Global
namespace Teamcity.CSharpInteractive
{
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Host;

    internal class ConsoleService: Console.ConsoleBase
    {
        private readonly IStdOut _stdOut;

        public ConsoleService(IStdOut stdOut) => _stdOut = stdOut;

        public override Task<Empty> WriteLine(WriteLineRequest request, ServerCallContext context)
        {
            _stdOut.WriteLine(new Text(request.Line, request.Color));
            return Task.FromResult(new Empty());
        }
    }
}
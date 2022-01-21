// ReSharper disable StringLiteralTypo
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios;

using Cmd;
using Docker;

[CollectionDefinition("Integration", DisableParallelization = true)]
public class DockerRun: ScenarioHostService
{
    [Fact]
    public void Run()
    {
        // $visible=true
        // $tag=12 Docker API
        // $priority=00
        // $description=Running in docker
        // {
        // Adds the namespace "Cmd" to use Command Line API
        // ## using Cmd;
        // Adds the namespace "Docker" to use Docker API
        // ## using Docker;

        // Resolves a build service
        var commandLine = GetService<ICommandLine>();

        // Creates some command line to run in a docker container
        var cmd = new CommandLine("whoami");

        // Runs the command line in a docker container
        var result = commandLine.Run(new Run(cmd, "mcr.microsoft.com/dotnet/sdk").WithAutoRemove(true));
        result.ShouldBe(0);
        // }
    }
}
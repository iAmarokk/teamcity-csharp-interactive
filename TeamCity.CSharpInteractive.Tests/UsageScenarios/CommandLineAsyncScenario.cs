// ReSharper disable StringLiteralTypo
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable RedundantAssignment
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios;

using System;
using HostApi;

[CollectionDefinition("Integration", DisableParallelization = true)]
[Trait("Integration", "true")]
public class CommandLineAsyncScenario : BaseScenario
{
    [SkippableFact]
    public async Task Run()
    {
        Skip.IfNot(Environment.OSVersion.Platform == PlatformID.Win32NT);
        Skip.IfNot(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")));

        // $visible=true
        // $tag=10 Command Line API
        // $priority=02
        // $description=Run a command line asynchronously
        // {
        // Adds the namespace "HostApi" to use Command Line API
        // ## using HostApi;

        int? exitCode = await GetService<ICommandLineRunner>().RunAsync(new CommandLine("cmd", "/C", "DIR"));
        
        // or the same thing using the extension method
        exitCode = await new CommandLine("cmd", "/c", "DIR").RunAsync();
        // }

        exitCode.HasValue.ShouldBeTrue();
    }
}
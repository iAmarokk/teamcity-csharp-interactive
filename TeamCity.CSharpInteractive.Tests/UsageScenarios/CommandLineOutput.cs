// ReSharper disable StringLiteralTypo
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios
{
    using System;
    using Contracts;
    using Shouldly;
    using Xunit;

    public class CommandLineOutput: Scenario
    {
        [SkippableFact]
        public void Run()
        {
            Skip.IfNot(Environment.OSVersion.Platform == PlatformID.Win32NT);

            // $visible=true
            // $tag=2 Command Line API
            // $priority=04
            // $description=Run and process output
            // {
            var lines = new System.Collections.Generic.List<string>();
            int? exitCode = GetService<ICommandLine>().Run(
                new CommandLine("cmd").AddArgs("/c", "SET").AddVars(("MyEnv", "MyVal")),
                i => lines.Add(i.Line));
            
            lines.ShouldContain("MyEnv=MyVal");
            // }
            
            exitCode.HasValue.ShouldBeTrue();
        }
    }
}
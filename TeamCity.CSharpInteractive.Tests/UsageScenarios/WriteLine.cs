// ReSharper disable StringLiteralTypo
// ReSharper disable SuggestVarOrType_BuiltInTypes
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios;

public class WriteLine: ScenarioHostService
{
    [Fact]
    public void Run()
    {
        // $visible=true
        // $tag=09 Logging
        // $priority=00
        // $description=Write a line to a build log
        // {
        WriteLine("Hello");
        // }
    }
}
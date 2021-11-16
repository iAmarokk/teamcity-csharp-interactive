// ReSharper disable StringLiteralTypo
// ReSharper disable SuggestVarOrType_BuiltInTypes
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios
{
    using System;
    using Contracts;
    using Xunit;

    public class GetServices: Scenario
    {
        [Fact]
        public void Run()
        {
            // $visible=true
            // $tag=08 Global state
            // $priority=03
            // $description=Get services
            // $header=This method might be used to get access to different APIs like [INuGet](TeamCity.CSharpInteractive.Contracts/INuGet.cs) or [ICommandLine](TeamCity.CSharpInteractive.Contracts/ICommandLine.cs).
            // $footer=Besides that, it is possible to get an instance of [System.IServiceProvider](https://docs.microsoft.com/en-US/dotnet/api/system.iserviceprovider) to access APIs.
            // {
            GetService<INuGet>();

            var serviceProvider = GetService<IServiceProvider>();
            serviceProvider.GetService(typeof(INuGet));
            // }
        }
    }
}
// ReSharper disable UnusedMember.Global
namespace TeamCity.CSharpInteractive.Contracts
{
    using System.Collections.Generic;

    public interface INuGet
    {
        IEnumerable<NuGetPackage> Restore(string packageId, string? versionRange = null, string? packagesPath = null);
    }
}
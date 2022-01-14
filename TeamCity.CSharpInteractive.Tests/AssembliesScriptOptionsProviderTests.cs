// ReSharper disable RedundantNameQualifier
namespace TeamCity.CSharpInteractive.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.Scripting;
using Moq;
using Shouldly;
using Xunit;

public class AssembliesScriptOptionsProviderTests
{
    private readonly Mock<IAssembliesProvider> _assembliesProvider = new();

    [Fact]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    public void ShouldAddReferencesForAssembliesWithLocation()
    {
        // Given
        var assembly1 = new Mock<Assembly>();
        var assembly2 = new Mock<Assembly>();
        assembly2.SetupGet(i => i.Location).Returns(Assembly.GetCallingAssembly().Location);
        _assembliesProvider.Setup(i => i.GetAssemblies(It.IsAny<IEnumerable<Type>>())).Returns(new [] { assembly1.Object, assembly2.Object });
        var provider = CreateInstance();

        // When
        var options = provider.Create(ScriptOptions.Default);

        // Then
        _assembliesProvider.Verify(i => i.GetAssemblies(new []
        {
            typeof(System.String),
            typeof(System.Collections.Generic.List<string>),
            typeof(System.IO.Path),
            typeof(System.Linq.Enumerable),
            typeof(System.Net.Http.HttpRequestMessage),
            typeof(System.Threading.Thread),
            typeof(System.Threading.Tasks.Task)
        }));
        
        options.MetadataReferences.Length.ShouldBe(ScriptOptions.Default.MetadataReferences.Length + 1);
    }
    
    [Fact]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    public void ShouldAddImports()
    {
        // Given
        _assembliesProvider.Setup(i => i.GetAssemblies(It.IsAny<IEnumerable<Type>>())).Returns(Enumerable.Empty<Assembly>());
        var provider = CreateInstance();

        // When
        var options = provider.Create(ScriptOptions.Default);

        // Then
        options.Imports.ShouldBe(AssembliesScriptOptionsProvider.Refs.Select(i => i.ns));
    }

    private AssembliesScriptOptionsProvider CreateInstance() =>
        new(Mock.Of<ILog<AssembliesScriptOptionsProvider>>(), _assembliesProvider.Object, CancellationToken.None);
}
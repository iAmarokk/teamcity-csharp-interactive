namespace Teamcity.CSharpInteractive.Tests.Integration
{
    using System;
    using System.IO;
    using System.Linq;
    using Core;
    using Shouldly;
    using Xunit;
    
    public class ScriptRunTests
    {
        private const int InitialLinesCount = 3;
            
        [Fact]
        public void ShouldRunEmptyScript()
        {
            // Given

            // When
            var result = TestTool.Run();
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Count.ShouldBe(InitialLinesCount);
        }
        
        [Fact]
        public void ShouldAddSystemNamespace()
        {
            // Given

            // When
            var result = TestTool.Run(@"Console.WriteLine(""Hello"");");
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Count.ShouldBe(InitialLinesCount + 1);
            result.StdOut.Contains("Hello").ShouldBeTrue();
        }
        
        [Theory]
        [InlineData(@"""Hello""", "Hello")]
        [InlineData("99", "99")]
        [InlineData("true", "True")]
        public void ShouldSupportWriteLine(string writeLineArg, string expectedOutput)
        {
            // Given

            // When
            var result = TestTool.Run(@$"WriteLine({writeLineArg});");
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Count.ShouldBe(InitialLinesCount + 1);
            result.StdOut.Contains(expectedOutput).ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportWriteLineWhenNoArgs()
        {
            // Given

            // When
            var result = TestTool.Run("WriteLine();");
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Count.ShouldBe(InitialLinesCount + 1);
            result.StdOut.Contains(string.Empty).ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportArgs()
        {
            // Given

            // When
            var result = TestTool.Run(
                Array.Empty<string>(),
                new []{"Abc", "Xyz"},
                @"WriteLine($""Args: {Args.Length}, {Args[0]}, {Args[1]}"");"
            );
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Contains("Args: 2, Abc, Xyz").ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportProps()
        {
            // Given

            // When
            var result = TestTool.Run(
                new []
                {
                    "--property", "Val1=Abc",
                    "/property", "val2=Xyz",
                    "-p", "val3=ASD",
                    "/p", "4=_"
                },
                Array.Empty<string>(),
                @"WriteLine(Props[""Val1""] + Props[""val2""] + Props[""val3""] + Props[""4""] + Props.Count);"
            );
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Contains("AbcXyzASD_4").ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportError()
        {
            // Given

            // When
            var result = TestTool.Run(@$"Error(""My error"");");
            
            // Then
            result.ExitCode.Value.ShouldBe(1);
            result.StdErr.ShouldBe(new []{ "My error" });
            result.StdOut.Count.ShouldBe(InitialLinesCount + 2);
            result.StdOut.Contains("My error").ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportWarning()
        {
            // Given

            // When
            var result = TestTool.Run(@$"Warning(""My warning"");");
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Count.ShouldBe(InitialLinesCount + 3);
            result.StdOut.Contains("My warning").ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportInfo()
        {
            // Given

            // When
            var result = TestTool.Run(@$"Info(""My info"");");
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Count.ShouldBe(InitialLinesCount + 1);
            result.StdOut.Contains("My info").ShouldBeTrue();
        }
        
        [Theory]
        [InlineData("nuget: IoC.Container, 1.3.6", "//1")]
        [InlineData("nuget:IoC.Container,1.3.6", "//1")]
        [InlineData("nuget: IoC.Container, [1.3.6, 2)", "//1")]
        [InlineData("nuget: IoC.Container", "container://1")]
        public void ShouldSupportNuGetRestore(string package, string name)
        {
            // Given

            // When
            var result = TestTool.Run(
                @$"#r ""{package}""",
                "using IoC;",
                "WriteLine(Container.Create());");
            
            // Then
            result.ExitCode.Value.ShouldBe(0);
            result.StdErr.ShouldBeEmpty();
            result.StdOut.Any(i => i.Trim() == "Installed:").ShouldBeTrue();
            result.StdOut.Contains(name).ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldSupportLoad()
        {
            // Given
            var fileSystem = Composer.Resolve<IFileSystem>();
            var refScriptFile = fileSystem.CreateTempFilePath();
            try
            {
                fileSystem.AppendAllLines(refScriptFile, new []{ @"Console.WriteLine(""Hello"");" });
                
                // When
                var result = TestTool.Run(@$"#load ""{refScriptFile}""");
                
                // Then
                result.ExitCode.Value.ShouldBe(0);
                result.StdErr.ShouldBeEmpty();
                result.StdOut.Count.ShouldBe(InitialLinesCount + 1);
                result.StdOut.Contains("Hello").ShouldBeTrue();
            }
            finally
            {
                fileSystem.DeleteFile(refScriptFile);
            }
        }
        
        [Fact]
        public void ShouldSupportLoadWhenRelativePath()
        {
            // Given
            var fileSystem = Composer.Resolve<IFileSystem>();
            var refScriptFile = fileSystem.CreateTempFilePath();
            try
            {
                fileSystem.AppendAllLines(refScriptFile, new []{ @"Console.WriteLine(""Hello"");" });
                
                // When
                var result = TestTool.Run(@$"#load ""{Path.GetFileName(refScriptFile)}""");
                
                // Then
                result.ExitCode.Value.ShouldBe(0);
                result.StdErr.ShouldBeEmpty();
                result.StdOut.Count.ShouldBe(InitialLinesCount + 1);
                result.StdOut.Contains("Hello").ShouldBeTrue();
            }
            finally
            {
                fileSystem.DeleteFile(refScriptFile);
            }
        }
        
        [Fact]
        public void ShouldProcessCompilationError()
        {
            // Given

            // When
            var result = TestTool.Run("i = 10;");
            
            // Then
            result.ExitCode.Value.ShouldBe(1);
            result.StdErr.Any(i => i.Contains("CS0103")).ShouldBeTrue();
            result.StdOut.Count.ShouldBe(InitialLinesCount + 2);
            result.StdOut.Any(i => i.Contains("CS0103")).ShouldBeTrue();
        }
        
        [Fact]
        public void ShouldProcessRuntimeException()
        {
            // Given

            // When
            var result = TestTool.Run(@"throw new Exception(""Test"");");
            
            // Then
            result.ExitCode.Value.ShouldBe(1);
            result.StdErr.Any(i => i.Contains("System.Exception: Test")).ShouldBeTrue();
            result.StdOut.Any(i => i.Contains("System.Exception: Test")).ShouldBeTrue();
        }
    }
}
namespace TeamCity.CSharpInteractive.Tests;

using System;
using Cmd;
using Contracts;
using Moq;
using Xunit;

public class ProcessMonitorTests
{
    private readonly Mock<ILog<ProcessMonitor>> _log = new();
    private readonly Mock<IEnvironment> _environment = new();
    private readonly Mock<IStartInfo> _startInfo = new();

    public ProcessMonitorTests()
    {
        _startInfo.SetupGet(i => i.ExecutablePath).Returns("Cm d");
        _startInfo.SetupGet(i => i.WorkingDirectory).Returns("W d");
        _startInfo.SetupGet(i => i.Args).Returns(new []{ "Arg1", "Arg 2" });
        _startInfo.SetupGet(i => i.Vars).Returns(new []{ ("Var1", "Val 1"), ("Var2", "Val 2") });
    }

    [Fact]
    public void ShouldLogHeaderOnStart()
    {
        // Given
        var monitor = CreateInstance();

        // When
        monitor.Started(_startInfo.Object, 99);

        // Then
        _log.Verify(i => i.Info(It.Is<Text[]>(text => 
            text.Length == 6
            && text[0].Value == "Starting process 99: "
            && text[1].Value == "\"Cm d\""
            && text[2] == Text.Space
            && text[3].Value == "Arg1"
            && text[4] == Text.Space
            && text[5].Value == "\"Arg 2\""
        )));
        
        _log.Verify(i => i.Info(It.Is<Text[]>(text => 
            text.Length == 2
            && text[0].Value == "in directory: "
            && text[1].Value == "\"W d\""
        )));
        
        _log.Verify(i => i.Trace(It.IsAny<Func<Text[]>>(), It.IsAny<string>()), Times.Never);
        _log.Verify(i => i.Warning(It.IsAny<Text[]>()), Times.Never);
        _log.Verify(i => i.Error(It.IsAny<ErrorId>(),It.IsAny<Text[]>()), Times.Never);
    }
    
    [Fact]
    public void ShouldLogCurrentWorkingDirectoryWhenWasNotSpecified()
    {
        // Given
        _startInfo.SetupGet(i => i.WorkingDirectory).Returns(string.Empty);
        _environment.Setup(i => i.GetPath(SpecialFolder.Working)).Returns("Cur Wd");
        var monitor = CreateInstance();

        // When
        monitor.Started(_startInfo.Object, 99);

        // Then
        _log.Verify(i => i.Info(It.Is<Text[]>(text => 
            text.Length == 6
            && text[0].Value == "Starting process 99: "
            && text[1].Value == "\"Cm d\""
            && text[2] == Text.Space
            && text[3].Value == "Arg1"
            && text[4] == Text.Space
            && text[5].Value == "\"Arg 2\""
        )));
        
        _log.Verify(i => i.Info(It.Is<Text[]>(text => 
            text.Length == 2
            && text[0].Value == "in directory: "
            && text[1].Value == "\"Cur Wd\""
        )));
        
        _log.Verify(i => i.Trace(It.IsAny<Func<Text[]>>(), It.IsAny<string>()), Times.Never);
        _log.Verify(i => i.Warning(It.IsAny<Text[]>()), Times.Never);
        _log.Verify(i => i.Error(It.IsAny<ErrorId>(),It.IsAny<Text[]>()), Times.Never);
    }

    [Theory]
    [InlineData(ProcessState.Succeeded, "finished successfully", Color.Success)]
    [InlineData(ProcessState.Unknown, "finished", Color.Highlighted)]
    public void ShouldLogWhenFinishedWithSuccess(ProcessState state, string stateDescription, Color color)
    {
        // Given
        var monitor = CreateInstance();
        monitor.Started(_startInfo.Object, 99);

        // When
        monitor.Finished(22, state, 33);

        // Then
        _log.Verify(i => i.Info(It.Is<Text[]>(text => 
            text.Length == 3
            && text[0].Value == "Process 99 " && text[0].Color == color
            && text[1].Value == stateDescription && text[0].Color == color
            && text[2].Value == $" (in {22} ms) with exit code {33}." && text[0].Color == color
        )));

        _log.Verify(i => i.Trace(It.IsAny<Func<Text[]>>(), It.IsAny<string>()), Times.Never);
        _log.Verify(i => i.Warning(It.IsAny<Text[]>()), Times.Never);
        _log.Verify(i => i.Error(It.IsAny<ErrorId>(),It.IsAny<Text[]>()), Times.Never);
    }

    [Fact]
    public void ShouldLogErrorWhenFailed()
    {
        // Given
        var monitor = CreateInstance();
        monitor.Started(_startInfo.Object, 99);

        // When
        monitor.Finished(22, ProcessState.Failed, 33);

        // Then
        _log.Verify(i => i.Error(ErrorId.Process, It.Is<Text[]>(text => 
            text.Length == 3
            && text[0].Value == "Process 99 "
            && text[1].Value == "failed"
            && text[2].Value == $" (in {22} ms) with exit code {33}."
        )));

        _log.Verify(i => i.Trace(It.IsAny<Func<Text[]>>(), It.IsAny<string>()), Times.Never);
        _log.Verify(i => i.Warning(It.IsAny<Text[]>()), Times.Never);
    }
    
    [Fact]
    public void ShouldLogErrorWhenFailedToStart()
    {
        // Given
        var monitor = CreateInstance();
        monitor.Started(_startInfo.Object, 99);

        // When
        monitor.Finished(22, ProcessState.Failed);

        // Then
        _log.Verify(i => i.Error(ErrorId.Process, It.Is<Text[]>(text => 
            text.Length == 2
            && text[1].Value == " - failed to start."
        )));

        _log.Verify(i => i.Trace(It.IsAny<Func<Text[]>>(), It.IsAny<string>()), Times.Never);
        _log.Verify(i => i.Warning(It.IsAny<Text[]>()), Times.Never);
    }
    
    [Fact]
    public void ShouldLogWarningWhenCanceled()
    {
        // Given
        var monitor = CreateInstance();
        monitor.Started(_startInfo.Object, 99);

        // When
        monitor.Finished(22, ProcessState.Canceled);

        // Then
        _log.Verify(i => i.Warning(It.Is<Text[]>(text => 
            text.Length == 2
            && text[1].Value == " - canceled."
        )));

        _log.Verify(i => i.Trace(It.IsAny<Func<Text[]>>(), It.IsAny<string>()), Times.Never);
        _log.Verify(i => i.Error(It.IsAny<ErrorId>(),It.IsAny<Text[]>()), Times.Never);
    }
    
    private ProcessMonitor CreateInstance() =>
        new(_log.Object, _environment.Object);
}
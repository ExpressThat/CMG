using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgRunGifArtifactOutputTests
{
    [Theory]
    [InlineData("GIF_DEBUG C:\\artifacts\\flow.debug.json")]
    [InlineData("GIF_TIMELINE C:\\artifacts\\flow.timeline.json")]
    [InlineData("GIF_CAPTURE_STATS path=\"C:\\\\artifacts\\\\flow.gif\" sourceFrames=20 retainedFrames=8")]
    [InlineData("GIF_WARN_UNCHANGED path=\"C:\\\\artifacts\\\\flow.gif\"")]
    [InlineData("GIF_WARN_BLANK path=\"C:\\\\artifacts\\\\flow.gif\"")]
    public void Run_RelaysMachineReadableGifSidecars(string artifactLine)
    {
        var method = typeof(CmgRunService).GetMethod(
            "IsGifArtifactOutput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(method);
        Assert.True((bool)method.Invoke(null, [artifactLine])!);
    }
}

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
    [InlineData("GIF_WARN_COLOR_PROFILE path=\"C:\\\\artifacts\\\\flow.gif\" profileChanges=1")]
    [InlineData("GIF_WARN_MULTIPLE_TARGETS line=3 action=click selector=\".save\" count=2")]
    [InlineData("GIF_WARN_TINY_TARGET line=3 action=click selector=\"#save\" width=8 height=8 threshold=16")]
    [InlineData("GIF_WARN_SCROLLED line=3 action=click selector=\"#save\" reason=offscreen-target")]
    [InlineData("GIF_WARN_NON_VISUAL line=4 action=recordCheckpoint options=pointerDuration")]
    [InlineData("GIF_WAIT_COMPRESSION path=\"C:\\\\artifacts\\\\flow.gif\" waits=2 savedMs=7600")]
    public void Run_RelaysMachineReadableGifSidecars(string artifactLine)
    {
        var method = typeof(CmgRunService).GetMethod(
            "IsGifArtifactOutput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(method);
        Assert.True((bool)method.Invoke(null, [artifactLine])!);
    }
}

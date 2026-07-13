using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgRunGifArtifactOutputTests
{
    [Theory]
    [InlineData("GIF_DEBUG C:\\artifacts\\flow.debug.json")]
    [InlineData("GIF_TIMELINE C:\\artifacts\\flow.timeline.json")]
    public void Run_RelaysMachineReadableGifSidecars(string artifactLine)
    {
        var method = typeof(CmgRunService).GetMethod(
            "IsGifArtifactOutput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(method);
        Assert.True((bool)method.Invoke(null, [artifactLine])!);
    }
}

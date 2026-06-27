using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifBlockTests
{
    [Fact]
    public void RunText_GifBlockRecordsNestedScriptWhenNoCommandGifIsActive()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");

        var result = Runner().RunText($$"""
        gif "block" output="{{Slash(path)}}" {
          showMessageBar "Inside block"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.True(File.Exists(path));
        Assert.Contains(result.StdoutLines, line => line.Contains($"GIF {Path.GetFullPath(path)}", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_GifBlockSuppressesNestedRecordingWhenCommandGifIsActive()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var nested = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");

        var result = Runner().RunText($$"""
        gif "block" output="{{Slash(nested)}}" {
          showMessageBar "Inside block"
        }
        """, "debug", new FakeAutomationClient(), new FileInfo(path));

        Assert.True(result.Success);
        Assert.True(File.Exists(path));
        Assert.False(File.Exists(nested));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_BLOCK_SUPPRESSED", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("recordVideo")]
    [InlineData("screencast")]
    public void RunText_ProviderRecordingAliasesUseGifRecorder(string action)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");

        var result = Runner().RunText($$"""
        {{action}} "block" output="{{Slash(path)}}" {
          showMessageBar "Inside block"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.True(File.Exists(path));
        Assert.Contains(result.StdoutLines, line => line.Contains($"GIF {Path.GetFullPath(path)}", StringComparison.Ordinal));
    }

    private static string Slash(string path) => path.Replace('\\', '/');

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

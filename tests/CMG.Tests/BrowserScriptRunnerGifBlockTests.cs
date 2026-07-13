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
        {{action}} "block" output="{{Slash(path)}}" quality=high {
          showMessageBar "Inside block"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.True(File.Exists(path));
        Assert.Contains(result.StdoutLines, line => line.Contains($"GIF {Path.GetFullPath(path)}", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_GifBlockRejectsInvalidQuality()
    {
        var result = Runner().RunText("""
        gif "block" quality=crunchy {
          showMessageBar "Inside block"
        }
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("gif quality must be one of", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void RunText_GifBlockAppliesEncoderControlsAndRetainsFrames()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-gif-controls-");
        var path = Path.Combine(directory.FullName, "controlled.gif");

        var result = Runner().RunText($$"""
        recording quality=archival dither=atkinson palette=local colors=96 keepFrames=true {
          gif "controlled" output="{{Slash(path)}}" {
            showMessageBar "Color evidence"
          }
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error);
        Assert.True(File.Exists(path));
        Assert.NotEmpty(Directory.GetFiles(Path.Combine(directory.FullName, "controlled.frames"), "*.png"));
        directory.Delete(recursive: true);
    }

    [Theory]
    [InlineData("dither=noisy", "dither=")]
    [InlineData("palette=shared-ish", "palette=")]
    [InlineData("colors=1", "colors=")]
    [InlineData("colors=257", "colors=")]
    public void RunText_GifBlockRejectsInvalidEncoderControls(string option, string expected)
    {
        var result = Runner().RunText($"gif \"block\" {option} {{ caption \"x\" }}", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains(expected, result.Error, StringComparison.Ordinal);
    }

    private static string Slash(string path) => path.Replace('\\', '/');

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

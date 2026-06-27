using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserGifBlockActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserGifBlockActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_RecordingBlockAliasesWriteGifArtifacts()
    {
        var recordVideo = fixture.OutputPath("record-video-alias.gif");
        var screencast = fixture.OutputPath("screencast-alias.gif");
        var script = fixture.CreateScript("recording-aliases.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        recordVideo "record-video" output="{{ScriptPath(recordVideo)}}" {
          scrollIntoView "#primary"
          click "#primary"
          caption "record video alias"
        }
        screencast "screencast" output="{{ScriptPath(screencast)}}" {
          scrollIntoView "#hover-card"
          hover "#hover-card"
          assertText "#hover-state" "hovered"
        }
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("GIF ");
        CmgE2eAssert.FileExists(recordVideo);
        CmgE2eAssert.FileExists(screencast);
    }

    [Fact]
    public void DirectScript_CommandGifSuppressesNestedGifBlockRecording()
    {
        var commandGif = fixture.OutputPath("command-level.gif");
        var nestedGif = fixture.OutputPath("nested-suppressed.gif");
        var script = fixture.CreateScript("command-gif-suppresses-block.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        gif "nested" output="{{ScriptPath(nestedGif)}}" {
          scrollIntoView "#primary"
          click "#primary"
        }
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script, "--gif", commandGif);

        result.ShouldPass();
        result.StdoutContains("GIF_BLOCK_SUPPRESSED 002");
        result.StdoutContains("GIF ");
        CmgE2eAssert.FileExists(commandGif);
        Assert.False(File.Exists(nestedGif));
    }

    [Fact]
    public void RunCommand_CommandGifSuppressesNestedRecordingAliases()
    {
        var gifDirectory = fixture.OutputPath("runner-gifs");
        var nestedGif = fixture.OutputPath("runner-nested-suppressed.gif");
        var report = fixture.OutputPath("runner-gif-suppression.json");
        var script = fixture.CreateScript("runner-gif-suppression.cmgscript", $$"""
        test "runner gif suppression" {
          navigate "{{fixture.FixtureHttpUri("index.html")}}"
          recordVideo "nested" output="{{ScriptPath(nestedGif)}}" {
            scrollIntoView "#primary"
            click "#primary"
          }
        }
        """);

        var result = fixture.Cli.Run("run", script, "--gif", gifDirectory, "--report-json", report);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner gif suppression");
        CmgE2eAssert.DirectoryHasFiles(gifDirectory, "*.gif");
        CmgE2eAssert.FileExists(report);
        Assert.Contains("GIF_BLOCK_SUPPRESSED", File.ReadAllText(report), StringComparison.Ordinal);
        Assert.False(File.Exists(nestedGif));
    }

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}

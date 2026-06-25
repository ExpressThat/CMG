using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerInitScriptActionTests
{
    [Fact]
    public void RunText_AddInitScriptRegistersInlineSource()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("addInitScript \"window.__ready = true;\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("window.__ready = true;", client.LastInitScript);
        Assert.Contains("INIT_SCRIPT 001 init-1", result.StdoutLines);
    }

    [Fact]
    public void RunText_AddInitScriptReadsFileSource()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.js");
        File.WriteAllText(path, "window.__fromFile = true;");
        var client = new FakeAutomationClient();

        var result = Runner().RunText($"addInitScript path=\"{Slash(path)}\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("window.__fromFile = true;", client.LastInitScript);
    }

    [Fact]
    public void RunText_AddInitScriptReportsMissingFile()
    {
        var result = Runner().RunText("addInitScript path=\"missing-init.js\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Init script file", result.Error);
    }

    [Fact]
    public void RunText_EvaluateOnNewDocumentAliasWorks()
    {
        var result = Runner().RunText("evaluateOnNewDocument \"window.__alias = true;\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("INIT_SCRIPT", string.Join('\n', result.StdoutLines));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static string Slash(string path) => path.Replace('\\', '/');
}

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

    [Fact]
    public void RunText_AddScriptTagInjectsInlineContent()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("addScriptTag \"window.__tag = true;\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.createElement('script')", client.LastExpression);
        Assert.Contains("window.__tag = true;", client.LastExpression);
        Assert.Contains("SCRIPT_TAG 001 content", result.StdoutLines);
    }

    [Fact]
    public void RunText_AddStyleTagUsesUrlLink()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("addStyleTag url=\"https://example.com/app.css\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.createElement('link')", client.LastExpression);
        Assert.Contains("rel = 'stylesheet'", client.LastExpression);
        Assert.Contains("STYLE_TAG 001 url", result.StdoutLines);
    }

    [Fact]
    public void RunText_AddTagReadsFileContent()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.css");
        File.WriteAllText(path, "body { color: red; }");
        var client = new FakeAutomationClient();

        var result = Runner().RunText($"addStyleTag path=\"{Slash(path)}\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("body { color: red; }", client.LastExpression);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static string Slash(string path) => path.Replace('\\', '/');
}

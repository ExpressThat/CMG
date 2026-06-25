using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNetworkTests
{
    [Fact]
    public void RunText_RouteInstallsNetworkPatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("route \"/api\" status=201 body=ok", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRoutes", client.LastExpression);
        Assert.Contains("ROUTE", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_ClearRoutesOutputsParseableLine()
    {
        var result = Runner().RunText("clearRoutes", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("ROUTES_CLEARED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForResponseOutputsResponseLine()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForResponse \"/api\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("waitForResponse", result.StdoutLines[0]);
        Assert.Contains(result.StdoutLines, line => line.Contains("RESPONSE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ExportHarWritesFile()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.har");
        var result = Runner().RunText($"exportHar path=\"{file.Replace('\\', '/')}\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.True(File.Exists(file));
        Assert.Contains("HAR_EXPORTED", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_ReplayHarInstallsRoutes()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.har");
        File.WriteAllText(file, """{"log":{"entries":[{"request":{"url":"/api"},"response":{"status":200,"content":{"text":"ok"}}}]}}""");
        var client = new FakeAutomationClient();

        var result = Runner().RunText($"replayHar path=\"{file.Replace('\\', '/')}\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRoutes.push", client.LastExpression);
        Assert.Contains("HAR_REPLAY", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_ReplayHarFailsWhenFileIsMissing()
    {
        var result = Runner().RunText("replayHar path=\"missing.har\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("was not found", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

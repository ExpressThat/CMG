using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerExposeFunctionTests
{
    [Fact]
    public void RunText_ExposeFunctionInstallsCurrentAndFutureFunction()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("exposeFunction cmgAdd \"(a, b) => a + b\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("window[\"cmgAdd\"]", client.LastExpression);
        Assert.Contains("window[\"cmgAdd\"]", client.LastInitScript);
        Assert.Contains("EXPOSED_FUNCTION 001 cmgAdd", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExposeBindingPassesSourceObject()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("exposeBinding cmgSource \"(source) => source.name\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("factory(source, ...args)", client.LastExpression);
    }

    [Fact]
    public void RunText_ExposeFunctionRejectsInvalidName()
    {
        var result = Runner().RunText("exposeFunction \"not-valid\" \"() => true\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("valid JavaScript identifier", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

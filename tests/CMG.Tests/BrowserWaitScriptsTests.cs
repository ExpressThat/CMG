using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserWaitScriptsTests
{
    [Fact]
    public void FunctionPollsUntilExpressionIsTruthy()
    {
        var script = BrowserWaitScripts.Function("window.ready === true", 250);

        Assert.Contains("window.ready === true", script);
        Assert.Contains("within 250ms", script);
        Assert.Contains("setTimeout(poll, 50)", script);
    }
}

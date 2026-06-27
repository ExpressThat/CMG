using CMG.Runner;

namespace CMG.Tests;

public sealed partial class CmgActionLowererTests
{
    [Fact]
    public void Lower_HttpCredentialActionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("setHttpCredentials \"user\" \"secret\"", Assert.Single(lowerer.Lower(Node("setHttpCredentials", ["user", "secret"], []))));
        Assert.Equal("clearHttpCredentials", Assert.Single(lowerer.Lower(Node("clearHttpCredentials", [], []))));
    }

    [Fact]
    public void Lower_ExposeFunctionActionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("exposeFunction \"cmg\" \"() => true\"", Assert.Single(lowerer.Lower(Node("exposeFunction", ["cmg", "() => true"], []))));
        Assert.Equal("exposeBinding \"cmg\" \"(source) => source.name\"", Assert.Single(lowerer.Lower(Node("exposeBinding", ["cmg", "(source) => source.name"], []))));
    }

    [Fact]
    public void Lower_WebSocketActionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("routeWebSocket \"/socket\"", Assert.Single(lowerer.Lower(Node("routeWebSocket", ["/socket"], []))));
        Assert.Equal("waitForWebSocket \"ready\"", Assert.Single(lowerer.Lower(Node("waitForWebSocket", ["ready"], []))));
    }

    [Fact]
    public void Lower_EnvironmentProviderActionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("setJavaScriptEnabled \"false\"", Assert.Single(lowerer.Lower(Node("setJavaScriptEnabled", ["false"], []))));
        Assert.Equal("emulateMedia media=\"print\"", Assert.Single(lowerer.Lower(Node("emulateMedia", [], new Dictionary<string, string> { ["media"] = "print" }))));
        Assert.Equal("bypassCSP \"true\"", Assert.Single(lowerer.Lower(Node("bypassCSP", ["true"], []))));
        Assert.Equal("serviceWorkers \"block\"", Assert.Single(lowerer.Lower(Node("serviceWorkers", ["block"], []))));
        Assert.Equal("setProxy \"https://proxy.local/?url=\"", Assert.Single(lowerer.Lower(Node("setProxy", ["https://proxy.local/?url="], []))));
    }

    [Fact]
    public void Lower_RichLocatorMarksElementThenUsesVisualSelector()
    {
        var lines = new CmgActionLowerer().Lower(Node("click", ["role=button"], []));

        Assert.Equal(3, lines.Count);
        Assert.StartsWith("evaluate", lines[0]);
        Assert.Contains("not actionable", lines[1]);
        Assert.Contains("data-cmg-locator-id", lines[2]);
    }
}

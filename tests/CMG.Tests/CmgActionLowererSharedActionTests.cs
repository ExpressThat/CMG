using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererSharedActionTests
{
    [Theory]
    [InlineData("waitForConsole", "saved", "waitForConsole \"saved\"")]
    [InlineData("capturePageErrors", null, "capturePageErrors")]
    [InlineData("waitForPageError", "boom", "waitForPageError \"boom\"")]
    [InlineData("captureDialogs", null, "captureDialogs")]
    [InlineData("setDialogBehavior", "dismiss", "setDialogBehavior \"dismiss\"")]
    [InlineData("waitForDialog", "Saved", "waitForDialog \"Saved\"")]
    [InlineData("route", "/api", "route \"/api\"")]
    [InlineData("intercept", "/api", "intercept \"/api\"")]
    [InlineData("waitForRequest", "/api", "waitForRequest \"/api\"")]
    [InlineData("waitForRequestFailed", "/api", "waitForRequestFailed \"/api\"")]
    [InlineData("frameClick", "#frame", "frameClick \"#frame\" \"#save\"", "#save")]
    [InlineData("tick", "250", "tick \"250\"")]
    [InlineData("keyDown", "Shift", "keyDown \"Shift\"")]
    [InlineData("keyUp", "Shift", "keyUp \"Shift\"")]
    [InlineData("insertText", "hello", "insertText \"hello\"")]
    [InlineData("navigate", "index.html", "navigate \"index.html\"")]
    [InlineData("reload", null, "reload")]
    [InlineData("goBack", null, "goBack")]
    [InlineData("goForward", null, "goForward")]
    [InlineData("waitForUrl", "/checkout", "waitForUrl \"/checkout\"")]
    [InlineData("waitForLoadState", "complete", "waitForLoadState \"complete\"")]
    [InlineData("waitForFunction", "window.ready", "waitForFunction \"window.ready\"")]
    [InlineData("waitForSelector", "#ready", "waitForSelector \"#ready\"")]
    [InlineData("waitForTimeout", "1", "waitForTimeout \"1\"")]
    [InlineData("setGeolocation", "51.5,-0.1", "setGeolocation \"51.5,-0.1\"")]
    [InlineData("grantPermissions", "geolocation", "grantPermissions \"geolocation\"")]
    [InlineData("clearPermissions", null, "clearPermissions")]
    [InlineData("mouseMove", "center", "mouseMove \"center\"")]
    [InlineData("mouseDown", "center", "mouseDown \"center\"")]
    [InlineData("mouseUp", "center", "mouseUp \"center\"")]
    [InlineData("localStorage", "clear", "localStorage \"clear\"")]
    [InlineData("sessionStorage", "clear", "sessionStorage \"clear\"")]
    [InlineData("cookie", "clear", "cookie \"clear\"")]
    [InlineData("waitForElement", "#ready", "waitForElement \"#ready\"")]
    [InlineData("clearContext", null, "clearContext")]
    [InlineData("newContext", "ctx", "newContext \"ctx\"")]
    [InlineData("useContext", "ctx-1", "useContext \"ctx-1\"")]
    [InlineData("listWorkers", null, "listWorkers")]
    [InlineData("workerIntercept", "/api", "workerIntercept \"/api\"")]
    [InlineData("startCoverage", null, "startCoverage")]
    [InlineData("stopCoverage", null, "stopCoverage")]
    [InlineData("addInitScript", "window.__ready = true;", "addInitScript \"window.__ready = true;\"")]
    [InlineData("evaluateOnNewDocument", "window.__ready = true;", "evaluateOnNewDocument \"window.__ready = true;\"")]
    [InlineData("setOffline", "true", "setOffline \"true\"")]
    [InlineData("clearExtraHTTPHeaders", null, "clearExtraHTTPHeaders")]
    public void Lower_SharedActionsPassThrough(string kind, string? arg, string expected, string? secondArg = null)
    {
        var args = new[] { arg, secondArg }.Where(value => value is not null).Cast<string>().ToArray();
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(kind, args)));

        Assert.Equal(expected, line);
    }

    [Fact]
    public void Lower_HarAccessibilityFileAndPdfOptionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("exportHar path=\"out.har\"", Assert.Single(lowerer.Lower(Node("exportHar", [], new Dictionary<string, string> { ["path"] = "out.har" }))));
        Assert.Equal("expectAccessible role=\"button\"", Assert.Single(lowerer.Lower(Node("expectAccessible", [], new Dictionary<string, string> { ["role"] = "button" }))));
        Assert.Equal("readFile \"payload\" path=\"fixtures/payload.json\"", Assert.Single(lowerer.Lower(Node("readFile", ["payload"], new Dictionary<string, string> { ["path"] = "fixtures/payload.json" }))));
        Assert.Equal("printPdf path=\"artifacts/page.pdf\"", Assert.Single(lowerer.Lower(Node("printPdf", [], new Dictionary<string, string> { ["path"] = "artifacts/page.pdf" }))));
    }

    [Fact]
    public void Lower_SetExtraHttpHeadersPassesHeaderPairsThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("setExtraHTTPHeaders", ["X-CMG", "yes"])));

        Assert.Equal("setExtraHTTPHeaders \"X-CMG\" \"yes\"", line);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyDictionary<string, string>? options = null) =>
        new(1, kind, args.FirstOrDefault() ?? kind, args, options ?? new Dictionary<string, string>(), []);
}

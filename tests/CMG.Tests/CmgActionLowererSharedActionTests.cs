using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererSharedActionTests
{
    [Theory]
    [InlineData("waitForConsole", "saved", "waitForConsole \"saved\"")]
    [InlineData("listConsole", null, "listConsole")]
    [InlineData("expectNoConsole", null, "expectNoConsole")]
    [InlineData("toHaveNoConsole", "deprecated", "toHaveNoConsole \"deprecated\"")]
    [InlineData("capturePageErrors", null, "capturePageErrors")]
    [InlineData("listPageErrors", null, "listPageErrors")]
    [InlineData("waitForPageError", "boom", "waitForPageError \"boom\"")]
    [InlineData("expectNoPageError", null, "expectNoPageError")]
    [InlineData("toHaveNoPageError", "boom", "toHaveNoPageError \"boom\"")]
    [InlineData("captureDialogs", null, "captureDialogs")]
    [InlineData("setDialogBehavior", "dismiss", "setDialogBehavior \"dismiss\"")]
    [InlineData("onDialog", "accept", "onDialog \"accept\"")]
    [InlineData("handleDialog", "dismiss", "handleDialog \"dismiss\"")]
    [InlineData("dialogBehavior", "accept", "dialogBehavior \"accept\"")]
    [InlineData("waitForDialog", "Saved", "waitForDialog \"Saved\"")]
    [InlineData("route", "/api", "route \"/api\"")]
    [InlineData("intercept", "/api", "intercept \"/api\"")]
    [InlineData("waitForRequest", "/api", "waitForRequest \"/api\"")]
    [InlineData("waitForRequestFinished", "/api", "waitForRequestFinished \"/api\"")]
    [InlineData("waitForRequestFailed", "/api", "waitForRequestFailed \"/api\"")]
    [InlineData("frameClick", "#frame", "frameClick \"#frame\" \"#save\"", "#save")]
    [InlineData("frameComputedStyle", "#frame", "frameComputedStyle \"#frame\" \"#save\" \"display\"", "#save", "display")]
    [InlineData("frameProperty", "#frame", "frameProperty \"#frame\" \"#save\" \"dataset.state\"", "#save", "dataset.state")]
    [InlineData("frameCount", "#frame", "frameCount \"#frame\" \".item\"", ".item")]
    [InlineData("tick", "250", "tick \"250\"")]
    [InlineData("keyDown", "Shift", "keyDown \"Shift\"")]
    [InlineData("keyUp", "Shift", "keyUp \"Shift\"")]
    [InlineData("insertText", "hello", "insertText \"hello\"")]
    [InlineData("keyboardShortcut", "Control+S", "keyboardShortcut \"Control+S\"")]
    [InlineData("navigate", "index.html", "navigate \"index.html\"")]
    [InlineData("goto", "index.html", "navigate \"index.html\"")]
    [InlineData("visit", "index.html", "navigate \"index.html\"")]
    [InlineData("reload", null, "reload")]
    [InlineData("goBack", null, "goBack")]
    [InlineData("goForward", null, "goForward")]
    [InlineData("waitForUrl", "/checkout", "waitForUrl \"/checkout\"")]
    [InlineData("waitForTitle", "Checkout", "waitForTitle \"Checkout\"")]
    [InlineData("toHaveURL", "/checkout", "expectUrl \"/checkout\"")]
    [InlineData("toHaveTitle", "Checkout", "expectTitle \"Checkout\"")]
    [InlineData("waitForLoadState", "complete", "waitForLoadState \"complete\"")]
    [InlineData("waitForNetworkIdle", null, "waitForNetworkIdle")]
    [InlineData("waitForNavigation", "/checkout", "waitForNavigation \"/checkout\"")]
    [InlineData("setViewportSize", null, "setViewport width=\"390\" height=\"844\"", null, "390", "844")]
    [InlineData("waitForPopup", null, "waitForPopup")]
    [InlineData("waitForFunction", "window.ready", "waitForFunction \"window.ready\"")]
    [InlineData("expectEval", "window.ready", "expectEval \"window.ready\"")]
    [InlineData("waitForSelector", "#ready", "waitForSelector \"#ready\"")]
    [InlineData("waitForTimeout", "1", "waitForTimeout \"1\"")]
    [InlineData("setGeolocation", "51.5,-0.1", "setGeolocation \"51.5,-0.1\"")]
    [InlineData("grantPermissions", "geolocation", "grantPermissions \"geolocation\"")]
    [InlineData("clearPermissions", null, "clearPermissions")]
    [InlineData("url", null, "url")]
    [InlineData("title", null, "title")]
    [InlineData("content", null, "content")]
    [InlineData("setContent", "<main>CMG</main>", "setContent \"<main>CMG</main>\"")]
    [InlineData("count", ".row", "count \".row\"")]
    [InlineData("boundingBox", "#card", "boundingBox \"#card\"")]
    [InlineData("allTextContents", ".item", "allTextContents \".item\"")]
    [InlineData("dispatchEvent", "#target", "dispatchEvent \"#target\" \"ready\"", "ready")]
    [InlineData("toHaveScreenshot", "#dialog", "expectScreenshot \"#dialog\"")]
    [InlineData("setInputFiles", "#file", "uploadFiles \"#file\" \"index.html\"", "index.html")]
    [InlineData("selectFile", "#file", "uploadFiles \"#file\" \"index.html\"", "index.html")]
    [InlineData("uploadFiles", "#file", "uploadFiles \"#file\" \"index.html\"", "index.html")]
    [InlineData("dragTo", "#source", "dragAndDrop \"#source\" \"#target\"", "#target")]
    [InlineData("pressSequentially", "#name", "pressSequentially \"#name\" \"CMG\"", "CMG")]
    [InlineData("mouseMove", "center", "mouseMove \"center\"")]
    [InlineData("mouseDown", "center", "mouseDown \"center\"")]
    [InlineData("mouseUp", "center", "mouseUp \"center\"")]
    [InlineData("scrollTo", "bottom", "scrollTo \"bottom\"")]
    [InlineData("scrollBy", "0", "scrollBy \"0\" \"120\"", "120")]
    [InlineData("wheel", "#pane", "wheel \"#pane\"")]
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
    [InlineData("startTracing", null, "startTracing")]
    [InlineData("stopTracing", null, "stopTracing")]
    [InlineData("addInitScript", "window.__ready = true;", "addInitScript \"window.__ready = true;\"")]
    [InlineData("evaluateOnNewDocument", "window.__ready = true;", "evaluateOnNewDocument \"window.__ready = true;\"")]
    [InlineData("addScriptTag", "window.__tag = true;", "addScriptTag \"window.__tag = true;\"")]
    [InlineData("addStyleTag", "body { color: red; }", "addStyleTag \"body { color: red; }\"")]
    [InlineData("setOffline", "true", "setOffline \"true\"")]
    [InlineData("clearExtraHTTPHeaders", null, "clearExtraHTTPHeaders")]
    [InlineData("toBeEditable", "#name", "tobeeditable \"#name\"")]
    [InlineData("expectInViewport", "#save", "expectinviewport \"#save\"")]
    [InlineData("apiRequest", "GET", "apiRequest \"GET\" \"https://example.test\"", "https://example.test")]
    [InlineData("recordCheckpoint", "after setup", "recordCheckpoint \"after setup\"")]
    [InlineData("intro", "Welcome", "intro \"Welcome\"")]
    [InlineData("outro", "Complete", "outro \"Complete\"")]
    [InlineData("fail", "Missing setup", "fail \"Missing setup\"")]
    public void Lower_SharedActionsPassThrough(
        string kind,
        string? arg,
        string expected,
        string? secondArg = null,
        string? thirdArg = null,
        string? fourthArg = null)
    {
        var args = new[] { arg, secondArg, thirdArg, fourthArg }.Where(value => value is not null).Cast<string>().ToArray();
        var lines = new CmgActionLowerer().Lower(Node(kind, args));
        var line = kind.Equals("pressSequentially", StringComparison.OrdinalIgnoreCase)
            ? lines.Last()
            : Assert.Single(lines);

        Assert.Equal(expected, line);
    }

    [Fact]
    public void Lower_ViewportAliasWithOptionsPassesThroughAsSetViewport()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("viewport", [], new Dictionary<string, string> { ["width"] = "390", ["height"] = "844" })));

        Assert.Equal("setViewport width=\"390\" height=\"844\"", line);
    }

    [Fact]
    public void Lower_HighlightUsesSharedSelectorActionPath()
    {
        var lines = new CmgActionLowerer().Lower(Node("highlight", ["#save"], new Dictionary<string, string> { ["message"] = "Save" }));

        Assert.Contains(lines, line => line.Contains("new Promise", StringComparison.Ordinal));
        Assert.Equal("highlight \"#save\" message=\"Save\"", lines.Last());
        Assert.Contains("message=\"Save\"", lines.Last(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("frameClick", "frameClick \"#frame\" \"role=button|Save\"")]
    [InlineData("frameComputedStyle", "frameComputedStyle \"#frame\" \"role=button|Save\" \"display\"")]
    [InlineData("frameProperty", "frameProperty \"#frame\" \"role=button|Save\" \"dataset.state\"")]
    [InlineData("frameToContainText", "frameToContainText \"#frame\" \"role=button|Save\" \"Saved\"")]
    public void Lower_FrameActionsNormalizeLocatorOptions(string kind, string expected)
    {
        string[] args = kind.Equals("frameClick", StringComparison.OrdinalIgnoreCase)
            ? ["#frame"]
            : kind.Equals("frameComputedStyle", StringComparison.OrdinalIgnoreCase)
                ? ["#frame", "display"]
                : kind.Equals("frameProperty", StringComparison.OrdinalIgnoreCase)
                    ? ["#frame", "dataset.state"]
                    : ["#frame", "Saved"];
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(
            kind,
            args,
            new Dictionary<string, string> { ["getByRole"] = "button|Save" })));

        Assert.Equal(expected, line);
    }

    [Fact]
    public void Lower_FrameEvaluateDoesNotNormalizeLocatorOptions()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(
            "frameEvaluate",
            ["#frame", "document.querySelector('[name=email]').value"],
            new Dictionary<string, string> { ["text"] = "Save" })));

        Assert.Equal("frameEvaluate \"#frame\" \"document.querySelector('[name=email]').value\" text=\"Save\"", line);
    }

    [Fact]
    public void Lower_ElementExpectationEscapesGeneratedNewlines()
    {
        var line = new CmgActionLowerer().Lower(Node("expectValue", ["#target", "Save"])).Last();

        Assert.DoesNotContain(Environment.NewLine, line);
        Assert.Contains("Expected value", line);
    }

    [Theory]
    [InlineData("toHaveClass", "class")]
    [InlineData("toHaveId", "id")]
    [InlineData("toHaveCSS", "css")]
    [InlineData("toHaveJSProperty", "property")]
    public void Lower_RichElementExpectationsGeneratePageAssertions(string kind, string mode)
    {
        var args = mode is "css" or "property"
            ? new[] { "#target", mode is "css" ? "display" : "dataset.ready", "block" }
            : ["#target", "ready"];
        var line = new CmgActionLowerer().Lower(Node(kind, args)).Last();

        Assert.Contains("evaluate", line);
        Assert.Contains(mode is "property" ? "Expected property" : mode is "css" ? "Expected CSS" : $"Expected {mode}", line);
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

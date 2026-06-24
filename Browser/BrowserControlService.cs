using CMG.Browser.Scripting;

namespace CMG.Browser;

public interface IBrowserControlService
{
    ElementResult GetElement(string selector, ElementOutputMode outputMode);

    ScriptRunResult RunScript(string file);
}

public sealed class BrowserControlService : IBrowserControlService
{
    private readonly BrowserStateStore stateStore;
    private readonly ChromeDevToolsClient devToolsClient;
    private readonly BrowserScriptRunner scriptRunner;

    public BrowserControlService(
        BrowserStateStore stateStore,
        ChromeDevToolsClient devToolsClient,
        BrowserScriptRunner scriptRunner)
    {
        this.stateStore = stateStore;
        this.devToolsClient = devToolsClient;
        this.scriptRunner = scriptRunner;
    }

    public ElementResult GetElement(string selector, ElementOutputMode outputMode)
    {
        var state = stateStore.Load();
        if (state is null)
        {
            return ElementResult.Fail("No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.");
        }

        try
        {
            return outputMode switch
            {
                ElementOutputMode.Html => ElementResult.ForHtml(devToolsClient.GetElementHtml(state.RemoteDebuggingUrl, selector)),
                ElementOutputMode.Screenshot => ElementResult.ForScreenshot(devToolsClient.GetElementScreenshot(state.RemoteDebuggingUrl, selector)),
                _ => ElementResult.Fail("Unsupported element output mode.")
            };
        }
        catch (ElementNotFoundException exception)
        {
            return ElementResult.Fail(exception.Message);
        }
        catch (ChromeDevToolsException exception)
        {
            return ElementResult.Fail(exception.Message);
        }
    }

    public ScriptRunResult RunScript(string file)
    {
        if (file is not "-" && !File.Exists(file))
        {
            return ScriptRunResult.Fail($"Script file '{file}' was not found.");
        }

        var state = stateStore.Load();
        if (state is null)
        {
            return ScriptRunResult.Fail("No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.");
        }

        return scriptRunner.Run(file, state.RemoteDebuggingUrl);
    }
}

public enum ElementOutputMode
{
    Html,
    Screenshot
}

public sealed record ElementResult(bool Success, string? Html, byte[]? ScreenshotPng, string? Error)
{
    public static ElementResult ForHtml(string html) => new(true, html, null, null);

    public static ElementResult ForScreenshot(byte[] screenshotPng) => new(true, null, screenshotPng, null);

    public static ElementResult Fail(string error) => new(false, null, null, error);
}

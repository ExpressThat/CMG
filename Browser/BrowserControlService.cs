using CMG.Browser.Scripting;

namespace CMG.Browser;

public interface IBrowserControlService
{
    ElementResult GetElement(BrowserKind browserKind, string selector, ElementOutputMode outputMode);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif);

    ScriptRunResult RunScriptAction(BrowserKind browserKind, string scriptLine);
}

public sealed class BrowserControlService : IBrowserControlService
{
    private readonly BrowserStateStore stateStore;
    private readonly BrowserAutomationClientFactory automationClientFactory;
    private readonly BrowserScriptRunner scriptRunner;

    public BrowserControlService(
        BrowserStateStore stateStore,
        BrowserAutomationClientFactory automationClientFactory,
        BrowserScriptRunner scriptRunner)
    {
        this.stateStore = stateStore;
        this.automationClientFactory = automationClientFactory;
        this.scriptRunner = scriptRunner;
    }

    public ElementResult GetElement(BrowserKind browserKind, string selector, ElementOutputMode outputMode)
    {
        var state = stateStore.Load(browserKind);
        if (state is null)
        {
            return ElementResult.Fail($"No CMG-controlled {browserKind.DisplayName()} instance is running. Run 'cmg {(browserKind is BrowserKind.Firefox ? "--firefox " : string.Empty)}browser launch' first.");
        }

        var automationClient = automationClientFactory.Create(browserKind);
        try
        {
            return outputMode switch
            {
                ElementOutputMode.Html => ElementResult.ForHtml(automationClient.GetElementHtml(state.RemoteDebuggingUrl, selector)),
                ElementOutputMode.Screenshot => ElementResult.ForScreenshot(automationClient.GetElementScreenshot(state.RemoteDebuggingUrl, selector)),
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

    public ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif)
    {
        if (file is not "-" && !File.Exists(file))
        {
            return ScriptRunResult.Fail($"Script file '{file}' was not found.");
        }

        var state = stateStore.Load(browserKind);
        if (state is null)
        {
            return ScriptRunResult.Fail($"No CMG-controlled {browserKind.DisplayName()} instance is running. Run 'cmg {(browserKind is BrowserKind.Firefox ? "--firefox " : string.Empty)}browser launch' first.");
        }

        return scriptRunner.Run(file, state.RemoteDebuggingUrl, automationClientFactory.Create(browserKind), gif);
    }

    public ScriptRunResult RunScriptAction(BrowserKind browserKind, string scriptLine)
    {
        var state = stateStore.Load(browserKind);
        if (state is null)
        {
            return ScriptRunResult.Fail($"No CMG-controlled {browserKind.DisplayName()} instance is running. Run 'cmg {(browserKind is BrowserKind.Firefox ? "--firefox " : string.Empty)}browser launch' first.");
        }

        return scriptRunner.RunText(scriptLine, state.RemoteDebuggingUrl, automationClientFactory.Create(browserKind));
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

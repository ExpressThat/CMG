using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;

namespace CMG.Browser;

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
        return GetElement(browserKind, port: null, selector, outputMode);
    }

    public ElementResult GetElement(BrowserKind browserKind, int? port, string selector, ElementOutputMode outputMode)
    {
        var state = stateStore.Load(browserKind, port);
        if (state is null)
        {
            return ElementResult.Fail(BuildBrowserNotRunningMessage(browserKind, port));
        }

        var automationClient = automationClientFactory.Create(browserKind);
        try
        {
            var resolved = ResolveSelector(state.RemoteDebuggingUrl, automationClient, selector);
            return outputMode switch
            {
                ElementOutputMode.Html => ElementResult.ForHtml(automationClient.GetElementHtml(state.RemoteDebuggingUrl, resolved)),
                ElementOutputMode.Screenshot => ElementResult.ForScreenshot(automationClient.GetElementScreenshot(state.RemoteDebuggingUrl, resolved)),
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
        catch (System.Net.Http.HttpRequestException exception)
        {
            return ElementResult.Fail(BuildBrowserConnectionMessage(browserKind, exception));
        }
    }

    public ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif)
    {
        return RunScript(browserKind, file, gif, trace: null);
    }

    public ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace)
    {
        return RunScript(browserKind, file, gif, trace, timeouts: null);
    }

    public ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts)
    {
        return RunScript(browserKind, file, gif, trace, timeouts, baseUrl: null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    public ScriptRunResult RunScript(
        BrowserKind browserKind,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest)
    {
        return RunScript(browserKind, port: null, file, gif, trace, timeouts, baseUrl, variables, gifQuality);
    }

    public ScriptRunResult RunScript(
        BrowserKind browserKind,
        int? port,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest)
    {
        if (file is not "-" && !File.Exists(file))
        {
            return ScriptRunResult.Fail($"Script file '{file}' was not found.");
        }

        var state = stateStore.Load(browserKind, port);
        if (state is null)
        {
            return ScriptRunResult.Fail(BuildBrowserNotRunningMessage(browserKind, port));
        }

        try
        {
            return scriptRunner.Run(file, state.RemoteDebuggingUrl, automationClientFactory.Create(browserKind), gif, trace, timeouts, baseUrl, variables, gifQuality);
        }
        catch (System.Net.Http.HttpRequestException exception)
        {
            return ScriptRunResult.Fail(BuildBrowserConnectionMessage(browserKind, exception));
        }
    }

    public ScriptRunResult RunScriptAction(BrowserKind browserKind, string scriptLine)
    {
        return RunScriptAction(browserKind, port: null, scriptLine);
    }

    public ScriptRunResult RunScriptAction(BrowserKind browserKind, int? port, string scriptLine)
    {
        var state = stateStore.Load(browserKind, port);
        if (state is null)
        {
            return ScriptRunResult.Fail(BuildBrowserNotRunningMessage(browserKind, port));
        }

        try
        {
            return scriptRunner.RunText(scriptLine, state.RemoteDebuggingUrl, automationClientFactory.Create(browserKind));
        }
        catch (System.Net.Http.HttpRequestException exception)
        {
            return ScriptRunResult.Fail(BuildBrowserConnectionMessage(browserKind, exception));
        }
    }

    public ScriptRunResult RunScriptText(
        BrowserKind browserKind,
        int? port,
        string script,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest)
    {
        var state = stateStore.Load(browserKind, port);
        if (state is null)
        {
            return ScriptRunResult.Fail(BuildBrowserNotRunningMessage(browserKind, port));
        }

        try
        {
            return scriptRunner.RunText(script, state.RemoteDebuggingUrl, automationClientFactory.Create(browserKind), gif, trace, timeouts, baseUrl, variables, gifQuality);
        }
        catch (System.Net.Http.HttpRequestException exception)
        {
            return ScriptRunResult.Fail(BuildBrowserConnectionMessage(browserKind, exception));
        }
    }

    private static string BuildBrowserNotRunningMessage(BrowserKind browserKind, int? port)
    {
        var selector = port is null ? string.Empty : $"--port {port} ";
        return $"No CMG-controlled {browserKind.DisplayName()} instance is running. Run 'cmg {browserKind.CommandOptionPrefix()}browser {selector}launch' first.";
    }

    private static string BuildBrowserConnectionMessage(BrowserKind browserKind, Exception exception) =>
        $"Could not connect to the CMG-controlled {browserKind.DisplayName()} browser endpoint. Run 'cmg {browserKind.CommandOptionPrefix()}browser launch' again. Reason: {exception.Message}";

    private static string ResolveSelector(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, string selector)
    {
        foreach (var expression in CmgLocator.PrefixExpressions(selector, lineNumber: 0))
        {
            automationClient.Evaluate(remoteDebuggingUrl, expression);
        }

        return CmgLocator.Resolve(selector, lineNumber: 0).Selector;
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

using System.Text.RegularExpressions;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private readonly BrowserScriptParser parser;

    public BrowserScriptRunner(BrowserScriptParser parser)
    {
        this.parser = parser;
    }

    public ScriptRunResult Run(string file, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptRunResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return RunParsedScript(readResult.Script ?? string.Empty, remoteDebuggingUrl, automationClient, gif);
    }

    public ScriptRunResult RunText(string script, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif = null)
    {
        return RunParsedScript(script, remoteDebuggingUrl, automationClient, gif);
    }

    private ScriptRunResult RunParsedScript(string script, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif)
    {
        var parseResult = parser.Parse(script);
        if (!parseResult.Success)
        {
            return ScriptRunResult.Fail(parseResult.Error ?? "Could not parse script.");
        }

        var context = new ScriptExecutionContext();
        var output = new List<string>();
        using var recorder = gif is null
            ? null
            : new ScriptGifRecorder(automationClient, new ScriptRecordingOptions(gif.FullName));

        recorder?.Start(remoteDebuggingUrl);

        for (var index = 0; index < parseResult.Actions.Count; index++)
        {
            var stepNumber = index + 1;
            var action = ExpandVariables(parseResult.Actions[index], context);

            try
            {
                recorder?.BeforeAction(action);
                var stepOutput = ExecuteAction(remoteDebuggingUrl, automationClient, action, context, recorder);
                recorder?.AfterAction(action);
                output.Add($"PASS {stepNumber:000} {action.Name} {FormatActionForLog(action)}".TrimEnd());
                output.AddRange(stepOutput);
            }
            catch (Exception exception) when (exception is ScriptExecutionException or ChromeDevToolsException or ElementNotFoundException)
            {
                FinishRecording(recorder, output);

                return ScriptRunResult.Fail(
                    $"Line {action.LineNumber}: {action.Name} failed. {exception.Message}",
                    output);
            }
        }

        FinishRecording(recorder, output);

        return ScriptRunResult.Ok(output);
    }

    private IReadOnlyList<string> ExecuteAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder)
    {
        if (action.Children.Count > 0 && !string.Equals(action.Name, "dragAndDrop", StringComparison.OrdinalIgnoreCase))
        {
            throw new ScriptExecutionException($"Action '{action.Name}' does not accept a block body.");
        }

        return action.Name.ToLowerInvariant() switch
        {
            "navigate" => ExecuteNavigate(remoteDebuggingUrl, automationClient, action),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action),
            "click" => ExecuteSelectorAction(action, selector => automationClient.Click(remoteDebuggingUrl, selector)),
            "type" => ExecuteType(remoteDebuggingUrl, automationClient, action, recorder),
            "clear" => ExecuteSelectorAction(action, selector => automationClient.Clear(remoteDebuggingUrl, selector)),
            "press" => ExecutePress(remoteDebuggingUrl, automationClient, action),
            "hover" => ExecuteSelectorAction(action, selector => automationClient.Hover(remoteDebuggingUrl, selector)),
            "scrollintoview" => ExecuteSelectorAction(action, selector => automationClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
            "select" => ExecuteSelect(remoteDebuggingUrl, automationClient, action),
            "showmessagebar" => ExecuteShowMessageBar(remoteDebuggingUrl, automationClient, action),
            "delay" => ExecuteDelay(action),
            "html" => ExecuteHtml(remoteDebuggingUrl, automationClient, action),
            "screenshot" => ExecuteScreenshot(remoteDebuggingUrl, automationClient, action),
            "screenshotpage" => ExecuteScreenshotPage(remoteDebuggingUrl, automationClient, action),
            "asserttext" => ExecuteAssertText(remoteDebuggingUrl, automationClient, action),
            "evaluate" => ExecuteEvaluate(remoteDebuggingUrl, automationClient, action),
            "setviewport" => ExecuteSetViewport(remoteDebuggingUrl, automationClient, action),
            "apirequest" => ExecuteApiRequest(action),
            "storagestate" => ExecuteStorageState(remoteDebuggingUrl, automationClient, action),
            "expectscreenshot" => ExecuteVisualAssertion(remoteDebuggingUrl, automationClient, action),
            "uploadfiles" => ExecuteUploadFiles(remoteDebuggingUrl, automationClient, action),
            "emulate" => ExecuteEmulate(remoteDebuggingUrl, automationClient, action),
            "download" => ExecuteDownload(remoteDebuggingUrl, automationClient, action),
            "waitfordownload" => ExecuteWaitForDownload(action),
            "captureconsole" => ExecuteCaptureConsole(remoteDebuggingUrl, automationClient, action),
            "waitforconsole" => ExecuteWaitForConsole(remoteDebuggingUrl, automationClient, action),
            "route" or "mockresponse" => ExecuteRoute(remoteDebuggingUrl, automationClient, action),
            "clearroutes" => ExecuteClearRoutes(remoteDebuggingUrl, automationClient, action),
            "waitforresponse" => ExecuteWaitForResponse(remoteDebuggingUrl, automationClient, action),
            "frameclick" or "frametype" or "framefill" or "framehover" or
            "framewaitforelement" or "frameasserttext" or "frameevaluate" =>
                ExecuteFrameAction(remoteDebuggingUrl, automationClient, action),
            "movemouse" => ExecuteMoveMouse(action, recorder, dragging: false),
            "draganddrop" => ExecuteDragAndDrop(remoteDebuggingUrl, automationClient, action, recorder),
            "listtabs" => ExecuteListTabs(remoteDebuggingUrl, automationClient, action),
            "opentab" => ExecuteOpenTab(remoteDebuggingUrl, automationClient, action),
            "waitfortab" => ExecuteWaitForTab(remoteDebuggingUrl, automationClient, action),
            "activatetab" => ExecuteActivateTab(remoteDebuggingUrl, automationClient, action),
            "closetab" => ExecuteCloseTab(remoteDebuggingUrl, automationClient, action),
            "set" => ExecuteSet(action, context),
            _ => throw new ScriptExecutionException($"Unknown action '{action.Name}'.")
        };
    }
}

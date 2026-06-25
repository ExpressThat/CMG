using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteNavigate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var finalUrl = automationClient.Navigate(remoteDebuggingUrl, NormalizeNavigationTarget(action.Arguments[0]));
        return string.IsNullOrWhiteSpace(finalUrl) ? [] : [$"NAVIGATED {action.LineNumber:000} {finalUrl}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForElement(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        automationClient.WaitForElement(remoteDebuggingUrl, action.Arguments[0], timeout);
        return [];
    }

    private static IReadOnlyList<string> ExecuteSelectorAction(BrowserScriptAction action, Action<string> execute)
    {
        RequireArgumentCount(action, 1, 1);
        execute(action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteType(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        RequireArgumentCount(action, 2, 2);
        if (recorder is null)
        {
            automationClient.Type(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
            return [];
        }

        recorder.CaptureClickPulse();
        automationClient.TypeProgressively(
            remoteDebuggingUrl,
            action.Arguments[0],
            action.Arguments[1],
            recorder.CaptureTypingFrame);

        return [];
    }

    private static IReadOnlyList<string> ExecutePress(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.Press(remoteDebuggingUrl, action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteKeyboardAction(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        switch (action.Name.ToLowerInvariant())
        {
            case "keydown":
                automationClient.KeyDown(remoteDebuggingUrl, action.Arguments[0]);
                return [$"KEY_DOWN {action.LineNumber:000} {action.Arguments[0]}"];
            case "keyup":
                automationClient.KeyUp(remoteDebuggingUrl, action.Arguments[0]);
                return [$"KEY_UP {action.LineNumber:000} {action.Arguments[0]}"];
            case "inserttext":
                automationClient.InsertText(remoteDebuggingUrl, action.Arguments[0]);
                return [$"TEXT_INSERTED {action.LineNumber:000} {action.Arguments[0].Length}"];
            default:
                throw new ScriptExecutionException($"Unknown keyboard action '{action.Name}'.");
        }
    }

    private static IReadOnlyList<string> ExecuteSelect(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        automationClient.Select(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteShowMessageBar(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.ShowMessageBar(remoteDebuggingUrl, action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteDelay(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        Thread.Sleep(ParsePositiveInt(action.Arguments[0], "delay"));
        return [];
    }

    private static IReadOnlyList<string> ExecuteHtml(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"HTML {action.LineNumber:000} {automationClient.GetElementHtml(remoteDebuggingUrl, action.Arguments[0])}"];
    }

    private static IReadOnlyList<string> ExecuteScreenshot(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var bytes = automationClient.GetElementScreenshot(remoteDebuggingUrl, action.Arguments[0]);
        return WriteScreenshotOutput(action, bytes);
    }

    private static IReadOnlyList<string> ExecuteScreenshotPage(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var bytes = automationClient.GetPageScreenshot(remoteDebuggingUrl, fullPage: GetBoolOption(action, "fullPage"));
        return WriteScreenshotOutput(action, bytes);
    }

    private static IReadOnlyList<string> ExecuteAssertText(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var timeout = GetIntOption(action, "timeout", 0);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        var text = string.Empty;

        do
        {
            text = automationClient.GetElementText(remoteDebuggingUrl, action.Arguments[0]);
            if (text.Contains(action.Arguments[1], StringComparison.Ordinal))
            {
                return [];
            }

            if (timeout <= 0)
            {
                break;
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        if (timeout > 0)
        {
            throw new ScriptExecutionException($"Expected text '{action.Arguments[1]}' was not found within {timeout}ms. Actual text: '{text}'.");
        }

        throw new ScriptExecutionException($"Expected text '{action.Arguments[1]}' was not found. Actual text: '{text}'.");
    }

    private static IReadOnlyList<string> ExecuteEvaluate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"EVALUATE {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, action.Arguments[0])}"];
    }

    private static IReadOnlyList<string> ExecutePageContentAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "url" => EvaluateNoArg(action, automationClient, remoteDebuggingUrl, "URL", "location.href"),
            "title" => EvaluateNoArg(action, automationClient, remoteDebuggingUrl, "TITLE", "document.title"),
            "content" => EvaluateNoArg(action, automationClient, remoteDebuggingUrl, "CONTENT", "document.documentElement.outerHTML"),
            "setcontent" => SetContent(action, automationClient, remoteDebuggingUrl),
            _ => throw new ScriptExecutionException($"Unknown page content action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> EvaluateNoArg(
        BrowserScriptAction action,
        IBrowserAutomationClient automationClient,
        string remoteDebuggingUrl,
        string label,
        string expression)
    {
        RequireArgumentCount(action, 0, 0);
        return [$"{label} {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, expression)}"];
    }

    private static IReadOnlyList<string> SetContent(
        BrowserScriptAction action,
        IBrowserAutomationClient automationClient,
        string remoteDebuggingUrl)
    {
        RequireArgumentCount(action, 1, 1);
        var html = QuoteScriptString(action.Arguments[0]);
        automationClient.Evaluate(remoteDebuggingUrl, $"document.open(); document.write({html}); document.close(); true");
        return [$"CONTENT_SET {action.LineNumber:000} length={action.Arguments[0].Length}"];
    }

    private static string QuoteScriptString(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)}\"";

    private static IReadOnlyList<string> ExecuteSetViewport(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.SetViewport(remoteDebuggingUrl, GetViewportOptions(action));
        return [];
    }

    private static ViewportOptions GetViewportOptions(BrowserScriptAction action) =>
        new(
            GetIntOption(action, "width", required: true),
            GetIntOption(action, "height", required: true),
            GetDoubleOption(action, "deviceScaleFactor", 1),
            GetBoolOption(action, "isMobile"),
            GetBoolOption(action, "hasTouch"));

    private static IReadOnlyList<string> ExecuteMoveMouse(BrowserScriptAction action, ScriptGifRecorder? recorder, bool dragging)
    {
        if (recorder is null)
        {
            throw new ScriptExecutionException("moveMouse requires script GIF recording. Run the script with --gif <path>.");
        }

        if (action.Children.Count > 0)
        {
            throw new ScriptExecutionException("moveMouse does not accept a block body.");
        }

        recorder.MoveMouse(action, dragging);
        return [];
    }
}

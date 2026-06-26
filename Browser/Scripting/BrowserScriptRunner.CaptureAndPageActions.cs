using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteScreenshot(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        var options = ScreenshotOptionsFor(action, fullPage: false);
        var bytes = automationClient.GetElementScreenshot(remoteDebuggingUrl, ResolveSelector(remoteDebuggingUrl, automationClient, action), options);
        return WriteScreenshotOutput(action, bytes, options.Type);
    }

    private static IReadOnlyList<string> ExecuteScreenshotPage(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var options = ScreenshotOptionsFor(action, GetBoolOption(action, "fullPage"));
        var bytes = automationClient.GetPageScreenshot(remoteDebuggingUrl, options: options);
        return WriteScreenshotOutput(action, bytes, options.Type);
    }

    private static ScreenshotOptions ScreenshotOptionsFor(BrowserScriptAction action, bool fullPage)
    {
        var type = ScreenshotImage.NormalizeType(action.Options.TryGetValue("type", out var rawType) ? rawType : "png");
        if (type is not ("png" or "jpeg"))
        {
            throw new ScriptExecutionException($"{action.Name} option type= must be png, jpeg, or jpg.");
        }

        var quality = action.Options.ContainsKey("quality") ? GetIntOption(action, "quality", required: true) : (int?)null;
        if (quality is not null && (quality < 0 || quality > 100))
        {
            throw new ScriptExecutionException($"{action.Name} option quality= must be between 0 and 100.");
        }
        if (quality is not null && type != "jpeg")
        {
            throw new ScriptExecutionException($"{action.Name} option quality= is only valid when type=jpeg.");
        }

        return new(type, quality, fullPage, GetBoolOption(action, "omitBackground"));
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

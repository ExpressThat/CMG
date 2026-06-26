using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteScreenshot(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        var options = ScreenshotOptionsFor(action, fullPage: false);
        if (options.Clip is not null)
        {
            throw new ScriptExecutionException("screenshot clip options are only valid with screenshotPage.");
        }
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var bytes = CaptureWithTemporaryStyle(
            action,
            remoteDebuggingUrl,
            automationClient,
            () => automationClient.GetElementScreenshot(remoteDebuggingUrl, selector, options));
        return WriteScreenshotOutput(action, bytes, options.Type);
    }

    private static IReadOnlyList<string> ExecuteScreenshotPage(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var options = ScreenshotOptionsFor(action, GetBoolOption(action, "fullPage"));
        var bytes = CaptureWithTemporaryStyle(
            action,
            remoteDebuggingUrl,
            automationClient,
            () => automationClient.GetPageScreenshot(remoteDebuggingUrl, options: options));
        return WriteScreenshotOutput(action, bytes, options.Type);
    }

    private static byte[] CaptureWithTemporaryStyle(
        BrowserScriptAction action,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        Func<byte[]> capture)
    {
        var id = AddTemporaryScreenshotStyle(action, remoteDebuggingUrl, automationClient);
        try
        {
            return capture();
        }
        finally
        {
            if (id is not null)
            {
                automationClient.Evaluate(remoteDebuggingUrl, $"document.querySelector('[data-cmg-screenshot-style=\"{id}\"]')?.remove(); true");
            }
        }
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

        return new(type, quality, fullPage, GetBoolOption(action, "omitBackground"), ScreenshotClipFor(action));
    }

    private static ScreenshotClip? ScreenshotClipFor(BrowserScriptAction action)
    {
        var hasClip = action.Options.ContainsKey("clipX") ||
            action.Options.ContainsKey("clipY") ||
            action.Options.ContainsKey("clipWidth") ||
            action.Options.ContainsKey("clipHeight");
        if (!hasClip)
        {
            return null;
        }

        var clip = new ScreenshotClip(
            GetClipOption(action, "clipX"),
            GetClipOption(action, "clipY"),
            GetClipOption(action, "clipWidth"),
            GetClipOption(action, "clipHeight"));
        if (clip.Width <= 0 || clip.Height <= 0)
        {
            throw new ScriptExecutionException($"{action.Name} clipWidth= and clipHeight= must be greater than 0.");
        }

        return clip;
    }

    private static double GetClipOption(BrowserScriptAction action, string name)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            throw new ScriptExecutionException($"{action.Name} clip options require clipX=, clipY=, clipWidth=, and clipHeight=.");
        }

        return double.TryParse(value, out var parsed) && parsed >= 0
            ? parsed
            : throw new ScriptExecutionException($"{action.Name} option {name}= must be zero or a positive number.");
    }

    private static string? AddTemporaryScreenshotStyle(
        BrowserScriptAction action,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient)
    {
        var inline = action.Options.TryGetValue("style", out var style) ? style : null;
        var file = action.Options.TryGetValue("stylePath", out var stylePath) ? stylePath : null;
        if (string.IsNullOrWhiteSpace(inline) && string.IsNullOrWhiteSpace(file))
        {
            return null;
        }
        if (!string.IsNullOrWhiteSpace(inline) && !string.IsNullOrWhiteSpace(file))
        {
            throw new ScriptExecutionException($"{action.Name} options style= and stylePath= cannot be used together.");
        }

        var css = !string.IsNullOrWhiteSpace(file) ? ReadScreenshotStyleFile(action, file!) : inline!;
        var id = $"cmg-{Guid.NewGuid():N}";
        automationClient.Evaluate(
            remoteDebuggingUrl,
            $"(() => {{ const style = document.createElement('style'); style.setAttribute('data-cmg-screenshot-style', '{id}'); style.textContent = {QuoteScriptString(css)}; document.documentElement.appendChild(style); return true; }})()");
        return id;
    }

    private static string ReadScreenshotStyleFile(BrowserScriptAction action, string path)
    {
        if (!File.Exists(path))
        {
            throw new ScriptExecutionException($"{action.Name} stylePath= file '{path}' did not exist.");
        }

        return File.ReadAllText(path);
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

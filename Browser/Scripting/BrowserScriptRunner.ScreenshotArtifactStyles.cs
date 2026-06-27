namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static string? AddTemporaryScreenshotStyle(
        BrowserScriptAction action,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient)
    {
        var css = ScreenshotArtifactCss(action);
        if (string.IsNullOrWhiteSpace(css))
        {
            return null;
        }

        var id = $"cmg-{Guid.NewGuid():N}";
        automationClient.Evaluate(
            remoteDebuggingUrl,
            $"(() => {{ const style = document.createElement('style'); style.setAttribute('data-cmg-screenshot-style', '{id}'); style.textContent = {QuoteScriptString(css)}; document.documentElement.appendChild(style); return true; }})()");
        return id;
    }

    private static string ScreenshotArtifactCss(BrowserScriptAction action)
    {
        var parts = new List<string>();
        AddUserScreenshotStyle(action, parts);
        AddAnimationScreenshotStyle(action, parts);
        AddCaretScreenshotStyle(action, parts);
        return string.Join(Environment.NewLine, parts);
    }

    private static void AddUserScreenshotStyle(BrowserScriptAction action, ICollection<string> parts)
    {
        var inline = action.Options.TryGetValue("style", out var style) ? style : null;
        var file = action.Options.TryGetValue("stylePath", out var stylePath) ? stylePath : null;
        if (!string.IsNullOrWhiteSpace(inline) && !string.IsNullOrWhiteSpace(file))
        {
            throw new ScriptExecutionException($"{action.Name} options style= and stylePath= cannot be used together.");
        }
        if (!string.IsNullOrWhiteSpace(file))
        {
            parts.Add(ReadScreenshotStyleFile(action, file!));
        }
        else if (!string.IsNullOrWhiteSpace(inline))
        {
            parts.Add(inline!);
        }
    }

    private static void AddAnimationScreenshotStyle(BrowserScriptAction action, ICollection<string> parts)
    {
        if (!action.Options.TryGetValue("animations", out var value))
        {
            return;
        }
        if (!value.Equals("disabled", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("allow", StringComparison.OrdinalIgnoreCase))
        {
            throw new ScriptExecutionException($"{action.Name} option animations= must be disabled or allow.");
        }
        if (value.Equals("disabled", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add("*,*::before,*::after{animation-duration:0s!important;animation-delay:0s!important;transition-duration:0s!important;transition-delay:0s!important;scroll-behavior:auto!important;}");
        }
    }

    private static void AddCaretScreenshotStyle(BrowserScriptAction action, ICollection<string> parts)
    {
        if (!action.Options.TryGetValue("caret", out var value))
        {
            return;
        }
        if (!value.Equals("hide", StringComparison.OrdinalIgnoreCase) &&
            !value.Equals("initial", StringComparison.OrdinalIgnoreCase))
        {
            throw new ScriptExecutionException($"{action.Name} option caret= must be hide or initial.");
        }
        if (value.Equals("hide", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add("*{caret-color:transparent!important;}");
        }
    }

    private static string ReadScreenshotStyleFile(BrowserScriptAction action, string path)
    {
        if (!File.Exists(path))
        {
            throw new ScriptExecutionException($"{action.Name} stylePath= file '{path}' did not exist.");
        }

        return File.ReadAllText(path);
    }
}

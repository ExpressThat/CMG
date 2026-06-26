using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteNavigate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var waitUntil = GetHistoryWaitUntil(action);
        var finalUrl = automationClient.Navigate(remoteDebuggingUrl, NormalizeNavigationTarget(action.Arguments[0]));
        if (string.IsNullOrWhiteSpace(finalUrl))
        {
            return [];
        }

        return [HistoryOutput(remoteDebuggingUrl, automationClient, action, "NAVIGATED", finalUrl, waitUntil, GetIntOption(action, "timeout", 5_000))];
    }

    private static IReadOnlyList<string> ExecuteWaitForElement(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        automationClient.WaitForElement(remoteDebuggingUrl, ResolveSelector(remoteDebuggingUrl, automationClient, action), timeout);
        return [];
    }

    private static IReadOnlyList<string> ExecuteSelectorAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        Action<string> execute)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        execute(ResolveSelector(remoteDebuggingUrl, automationClient, action));
        return [];
    }

    private static IReadOnlyList<string> ExecuteType(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        if (recorder is null)
        {
            TypeWithoutRecorder(remoteDebuggingUrl, automationClient, action, selector);
            return [];
        }

        recorder.CaptureClickPulse();
        automationClient.TypeProgressively(
            remoteDebuggingUrl,
            selector,
            action.Arguments[1],
            GetTypingDelay(action),
            recorder.CaptureTypingFrame);

        return [];
    }

    private static IReadOnlyList<string> ExecuteSelect(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        if (HasSelectOption(action))
        {
            action = NormalizeSelectorArgument(action);
            RequireArgumentCount(action, 1, 1);
            var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
            automationClient.Evaluate(remoteDebuggingUrl, BuildSelectOptionExpression(selector, SelectOptionSpec.From(action)));
            return [];
        }

        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);
        automationClient.Select(remoteDebuggingUrl, ResolveSelector(remoteDebuggingUrl, automationClient, action), action.Arguments[1]);
        return [];
    }

    private static bool HasSelectOption(BrowserScriptAction action) =>
        action.Options.Keys.Any(key => key is "optionLabel" or "optionValue" or "index");

    private static string BuildSelectOptionExpression(string selector, SelectOptionSpec spec) =>
        "(() => { "
        + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
        + $"if (!element) throw new Error('No element matched selector {selector}'); "
        + "const options = Array.from(element.options ?? []); "
        + $"const target = {spec.TargetExpression}; "
        + $"if (!target) throw new Error({QuoteScriptString($"No option matched {spec.Description}.")}); "
        + "if (!element.multiple) for (const option of options) option.selected = false; "
        + "target.selected = true; element.value = target.value; "
        + "element.dispatchEvent(new Event('input', { bubbles: true })); "
        + "element.dispatchEvent(new Event('change', { bubbles: true })); "
        + "return target.value; })()";

    private sealed record SelectOptionSpec(string TargetExpression, string Description)
    {
        public static SelectOptionSpec From(BrowserScriptAction action)
        {
            if (action.Options.TryGetValue("index", out var rawIndex))
            {
                var index = ParseNonNegativeIndex(rawIndex);
                return new($"options[{index}]", $"index {index}");
            }

            if (action.Options.TryGetValue("optionLabel", out var label))
            {
                var value = QuoteScriptString(label);
                return new($"options.find(option => option.label === {value} || option.textContent.trim() === {value})", $"label '{label}'");
            }

            var optionValue = action.Options.GetValueOrDefault("optionValue")
                ?? throw new ScriptExecutionException($"{action.Name} requires optionLabel=, optionValue=, or index= when no value argument is provided.");
            var quoted = QuoteScriptString(optionValue);
            return new($"options.find(option => option.value === {quoted})", $"value '{optionValue}'");
        }

        private static int ParseNonNegativeIndex(string value) =>
            int.TryParse(value, out var index) && index >= 0
                ? index
                : throw new ScriptExecutionException("selectOption index= must be zero or greater.");
    }

    private static void TypeWithoutRecorder(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string selector)
    {
        if (action.Options.ContainsKey("delay"))
        {
            automationClient.TypeProgressively(remoteDebuggingUrl, selector, action.Arguments[1], GetTypingDelay(action));
            return;
        }

        automationClient.Type(remoteDebuggingUrl, selector, action.Arguments[1]);
    }

    private static int GetTypingDelay(BrowserScriptAction action)
    {
        var delay = GetIntOption(action, "delay", 80);
        return delay >= 0
            ? delay
            : throw new ScriptExecutionException($"{action.Name} option delay= must be zero or greater.");
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

    private static IReadOnlyList<string> ExecuteFail(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        throw new ScriptExecutionException(string.Join(' ', action.Arguments));
    }

    private static IReadOnlyList<string> ExecuteHtml(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        return [$"HTML {action.LineNumber:000} {automationClient.GetElementHtml(remoteDebuggingUrl, ResolveSelector(remoteDebuggingUrl, automationClient, action))}"];
    }

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

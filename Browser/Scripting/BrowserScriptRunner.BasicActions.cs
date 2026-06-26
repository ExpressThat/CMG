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

    private static IReadOnlyList<string> ExecuteEvaluate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"EVALUATE {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, action.Arguments[0])}"];
    }
}

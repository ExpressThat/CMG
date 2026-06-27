namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteForEachSelector(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 2, 2);
        var variable = action.Arguments[0];
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, NormalizeSelectorArgument(
            action with { Arguments = [action.Arguments[1]] }));
        var count = CountSelectorMatches(remoteDebuggingUrl, automationClient, selector);

        for (var index = 0; index < count; index++)
        {
            var itemSelector = $"#__cmg_foreach_{action.LineNumber}_{index}";
            automationClient.Evaluate(remoteDebuggingUrl, MarkSelectorMatch(selector, index, action.LineNumber));
            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output, [(variable, itemSelector), ("index", index.ToString())]);
            if (control == "break") break;
        }
    }

    private static int CountSelectorMatches(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string selector)
    {
        var result = automationClient.Evaluate(
            remoteDebuggingUrl,
            $"{CMG.Browser.BrowserDomScripts.QueryAll(selector)}.length");
        return int.TryParse(result, out var count) ? count : 0;
    }

    private static string MarkSelectorMatch(string selector, int index, int lineNumber) =>
        "(() => { "
        + $"const elements = Array.from({CMG.Browser.BrowserDomScripts.QueryAll(selector)}); "
        + $"if (!elements[{index}]) throw new Error('No element at index {index} for selector {selector}'); "
        + $"elements[{index}].id = '__cmg_foreach_{lineNumber}_{index}'; "
        + "return true; })()";
}

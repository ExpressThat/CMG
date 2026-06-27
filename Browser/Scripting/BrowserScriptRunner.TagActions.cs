namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteAddTag(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            throw new ScriptExecutionException($"{action.Name} expects at most 1 positional argument.");
        }

        var spec = ReadTagSpec(action);
        automationClient.Evaluate(remoteDebuggingUrl, BuildTagExpression(action.Name, spec));
        var label = action.Name.Equals("addStyleTag", StringComparison.OrdinalIgnoreCase) ? "STYLE_TAG" : "SCRIPT_TAG";
        return [$"{label} {action.LineNumber:000} {spec.Kind}"];
    }

    private static TagSpec ReadTagSpec(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("url", out var url) && !string.IsNullOrWhiteSpace(url))
        {
            return new("url", url);
        }

        if (action.Options.TryGetValue("path", out var path) && !string.IsNullOrWhiteSpace(path))
        {
            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                throw new ScriptExecutionException($"Tag file '{fullPath}' was not found.");
            }

            return new("content", File.ReadAllText(fullPath));
        }

        if (action.Options.TryGetValue("content", out var content))
        {
            return new("content", content);
        }

        return action.Arguments.Count is 1
            ? new("content", action.Arguments[0])
            : throw new ScriptExecutionException($"{action.Name} requires url=, path=, content=, or inline content.");
    }

    private static string BuildTagExpression(string actionName, TagSpec spec)
    {
        var isStyle = actionName.Equals("addStyleTag", StringComparison.OrdinalIgnoreCase);
        var tag = isStyle && spec.Kind is "url" ? "link" : isStyle ? "style" : "script";
        var body = spec.Kind is "url"
            ? isStyle
                ? $"element.rel = 'stylesheet'; element.href = {QuoteScriptString(spec.Value)};"
                : $"element.src = {QuoteScriptString(spec.Value)};"
            : $"element.textContent = {QuoteScriptString(spec.Value)};";
        return $"(() => {{ const element = document.createElement('{tag}'); {body} document.head.appendChild(element); return true; }})()";
    }

    private sealed record TagSpec(string Kind, string Value);
}

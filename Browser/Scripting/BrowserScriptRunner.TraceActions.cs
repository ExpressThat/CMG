namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteTraceAction(BrowserScriptAction action, ScriptExecutionContext context)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "starttracing" or "tracingstart" => StartTracing(action, context),
            "stoptracing" or "tracingstop" => StopTracing(action, context),
            _ => throw new ScriptExecutionException($"Unknown trace action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> StartTracing(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 0, 0);
        if (context.Trace?.IsActive is true)
        {
            if (context.Trace.SuppressNested)
            {
                return [$"TRACE_BLOCK_SUPPRESSED {action.LineNumber:000}"];
            }

            throw new ScriptExecutionException("Tracing is already active.");
        }

        var output = TracePath(action);
        context.Trace = new BrowserScriptTraceSession(output);
        return [$"TRACE_STARTED {action.LineNumber:000}{FormatTracePath(output)}"];
    }

    private static IReadOnlyList<string> StopTracing(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 0, 0);
        if (context.Trace?.IsActive is not true)
        {
            throw new ScriptExecutionException("Tracing is not active.");
        }

        if (context.Trace.SuppressNested)
        {
            return [$"TRACE_BLOCK_SUPPRESSED {action.LineNumber:000}"];
        }

        var path = context.Trace.Finish(TracePath(action), success: true, error: null);
        return [$"TRACE {action.LineNumber:000} {path}"];
    }

    private static string? TracePath(BrowserScriptAction action) =>
        action.Options.TryGetValue("path", out var path) && !string.IsNullOrWhiteSpace(path)
            ? path
            : action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output) ? output : null;

    private static string FormatTracePath(string? path) =>
        string.IsNullOrWhiteSpace(path) ? string.Empty : $" {Path.GetFullPath(path)}";
}

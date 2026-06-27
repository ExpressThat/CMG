using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private bool RunActions(
        IReadOnlyList<CmgNode> actions,
        string remoteDebuggingUrl,
        FileInfo? gif,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        List<string> output,
        List<CmgStepResult> steps,
        out string? error)
    {
        var pending = new List<string>();
        var pendingLineMap = new Dictionary<int, int>();
        foreach (var action in actions)
        {
            if (TryRunDirectAction(action, remoteDebuggingUrl, gif, timeouts, baseUrl, pending, pendingLineMap, output, steps, out error))
            {
                if (error is not null)
                {
                    return false;
                }

                continue;
            }

            AddPending(pending, pendingLineMap, action, lowerer.Lower(action));
        }

        var final = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif, timeouts, baseUrl);
        return AppendResult(final.Result, final.LineMap, output, steps, actions.LastOrDefault(), gif, out error);
    }

    private bool TryRunDirectAction(
        CmgNode action,
        string remoteDebuggingUrl,
        FileInfo? gif,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        List<string> pending,
        Dictionary<int, int> pendingLineMap,
        List<string> output,
        List<CmgStepResult> steps,
        out string? error)
    {
        error = null;
        if (!IsDirectAction(action))
        {
            return false;
        }

        var flush = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif, timeouts, baseUrl);
        if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif, out error))
        {
            return true;
        }

        var step = RunDirectAction(action, remoteDebuggingUrl, timeouts);
        output.AddRange(step.Output);
        steps.Add(step);
        error = step.Success ? null : step.Error;
        return true;
    }

    private CmgStepResult RunDirectAction(CmgNode action, string remoteDebuggingUrl, ScriptTimeoutOptions? timeouts)
    {
        if (action.Kind.Equals("apiRequest", StringComparison.OrdinalIgnoreCase))
        {
            return apiRequestRunner.Run(ApplyRunTimeoutDefault(action, timeouts));
        }

        if (action.Kind.Equals("storageState", StringComparison.OrdinalIgnoreCase))
        {
            return storageStateRunner.Run(action, remoteDebuggingUrl, automationClient);
        }

        if (action.Kind.Equals("uploadFiles", StringComparison.OrdinalIgnoreCase))
        {
            return uploadRunner.Run(action, remoteDebuggingUrl, automationClient);
        }

        return visualAssertionRunner.Run(action, remoteDebuggingUrl, automationClient);
    }

    private static bool IsDirectAction(CmgNode action) =>
        action.Kind.Equals("apiRequest", StringComparison.OrdinalIgnoreCase) ||
        action.Kind.Equals("storageState", StringComparison.OrdinalIgnoreCase) ||
        action.Kind.Equals("expectScreenshot", StringComparison.OrdinalIgnoreCase) ||
        action.Kind.Equals("uploadFiles", StringComparison.OrdinalIgnoreCase);
}

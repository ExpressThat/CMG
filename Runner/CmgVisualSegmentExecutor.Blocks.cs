using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private bool RunActions(
        IReadOnlyList<CmgNode> actions,
        string remoteDebuggingUrl,
        FileInfo? gif,
        List<string> output,
        List<CmgStepResult> steps,
        out string? error)
    {
        var pending = new List<string>();
        foreach (var action in actions)
        {
            if (TryRunDirectAction(action, remoteDebuggingUrl, gif, pending, output, steps, out error))
            {
                if (error is not null)
                {
                    return false;
                }

                continue;
            }

            pending.AddRange(lowerer.Lower(action));
        }

        return AppendResult(RunLines(pending, remoteDebuggingUrl, gif), output, steps, actions.LastOrDefault(), gif, out error);
    }

    private bool TryRunDirectAction(
        CmgNode action,
        string remoteDebuggingUrl,
        FileInfo? gif,
        List<string> pending,
        List<string> output,
        List<CmgStepResult> steps,
        out string? error)
    {
        error = null;
        if (!IsDirectAction(action))
        {
            return false;
        }

        var flush = RunLines(pending, remoteDebuggingUrl, gif);
        if (!AppendResult(flush, output, steps, action, gif, out error))
        {
            return true;
        }

        var step = RunDirectAction(action, remoteDebuggingUrl);
        output.AddRange(step.Output);
        steps.Add(step);
        error = step.Success ? null : step.Error;
        return true;
    }

    private CmgStepResult RunDirectAction(CmgNode action, string remoteDebuggingUrl)
    {
        if (action.Kind.Equals("apiRequest", StringComparison.OrdinalIgnoreCase))
        {
            return apiRequestRunner.Run(action);
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

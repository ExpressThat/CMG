namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteTry(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        IReadOnlyList<BrowserScriptAction> branches,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        var tryBranch = branches[0];
        var catchBranch = branches.FirstOrDefault(branch => branch.Name.Equals("catch", StringComparison.OrdinalIgnoreCase));
        var finallyBranch = branches.FirstOrDefault(branch => branch.Name.Equals("finally", StringComparison.OrdinalIgnoreCase));
        ValidateTryBranches(branches, catchBranch, finallyBranch);

        ScriptActionFailedException? failure = null;
        try
        {
            WithMacroScope(context, () =>
                context.PushExecutionContext("try", () =>
                    ExecuteActions(remoteDebuggingUrl, automationClient, tryBranch.Children, context, recorder, output)));
        }
        catch (ScriptActionFailedException exception)
        {
            failure = exception;
            if (catchBranch is not null)
            {
                ExecuteCatch(remoteDebuggingUrl, automationClient, catchBranch, exception.Message, context, recorder, output);
                failure = null;
            }
        }
        finally
        {
            if (finallyBranch is not null)
            {
                WithMacroScope(context, () =>
                    context.PushExecutionContext("finally", () =>
                        ExecuteActions(remoteDebuggingUrl, automationClient, finallyBranch.Children, context, recorder, output)));
            }
        }

        if (failure is not null)
        {
            throw failure;
        }
    }

    private void ExecuteCatch(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string error,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 0, 1);
        var values = action.Arguments.Count is 1 ? [(action.Arguments[0], error)] : Array.Empty<(string, string)>();
        WithVariables(context, values, () =>
            context.PushExecutionContext("catch", () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output)));
    }

    private static void ValidateTryBranches(
        IReadOnlyList<BrowserScriptAction> branches,
        BrowserScriptAction? catchBranch,
        BrowserScriptAction? finallyBranch)
    {
        if (branches.Count(branch => branch.Name.Equals("catch", StringComparison.OrdinalIgnoreCase)) > 1)
        {
            throw new ScriptExecutionException("try can have only one catch block.");
        }

        if (branches.Count(branch => branch.Name.Equals("finally", StringComparison.OrdinalIgnoreCase)) > 1)
        {
            throw new ScriptExecutionException("try can have only one finally block.");
        }

        if (catchBranch is not null &&
            finallyBranch is not null &&
            IndexOf(branches, catchBranch) > IndexOf(branches, finallyBranch))
        {
            throw new ScriptExecutionException("catch must appear before finally.");
        }
    }

    private static int IndexOf(IReadOnlyList<BrowserScriptAction> branches, BrowserScriptAction action)
    {
        for (var index = 0; index < branches.Count; index++)
        {
            if (ReferenceEquals(branches[index], action))
            {
                return index;
            }
        }

        return -1;
    }
}

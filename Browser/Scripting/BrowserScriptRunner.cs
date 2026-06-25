using System.Text.RegularExpressions;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private readonly BrowserScriptParser parser;

    public BrowserScriptRunner(BrowserScriptParser parser)
    {
        this.parser = parser;
    }

    public ScriptRunResult Run(string file, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptRunResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return RunParsedScript(readResult.Script ?? string.Empty, remoteDebuggingUrl, automationClient, gif);
    }

    public ScriptRunResult RunText(string script, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif = null)
    {
        return RunParsedScript(script, remoteDebuggingUrl, automationClient, gif);
    }

    private ScriptRunResult RunParsedScript(string script, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif)
    {
        var importResult = ScriptImportExpander.Expand(script, Directory.GetCurrentDirectory());
        if (!importResult.Success)
        {
            return ScriptRunResult.Fail(importResult.Error ?? "Could not import script.");
        }

        script = importResult.Script ?? string.Empty;
        var parseResult = parser.Parse(script);
        if (!parseResult.Success)
        {
            return ScriptRunResult.Fail(parseResult.Error ?? "Could not parse script.");
        }

        var context = new ScriptExecutionContext();
        var output = new List<string>();
        using var recorder = gif is null
            ? null
            : new ScriptGifRecorder(automationClient, new ScriptRecordingOptions(gif.FullName));

        recorder?.Start(remoteDebuggingUrl);

        try
        {
            ExecuteActions(remoteDebuggingUrl, automationClient, parseResult.Actions, context, recorder, output);
        }
        catch (ScriptActionFailedException exception)
        {
            FinishRecording(recorder, output);
            return ScriptRunResult.Fail(exception.Message, output);
        }
        catch (LoopControlException exception)
        {
            FinishRecording(recorder, output);
            return ScriptRunResult.Fail($"{exception.Kind} must be inside a loop.", output);
        }

        FinishRecording(recorder, output);

        return ScriptRunResult.Ok(output);
    }

    private void ExecuteActions(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        IReadOnlyList<BrowserScriptAction> actions,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder,
        List<string> output)
    {
        for (var index = 0; index < actions.Count; index++)
        {
            if (actions[index].Name.Equals("if", StringComparison.OrdinalIgnoreCase))
            {
                var branches = CollectBranches(actions, ref index);
                ExecuteIf(remoteDebuggingUrl, automationClient, branches, context, recorder, output);
                continue;
            }

            if (actions[index].Name.Equals("try", StringComparison.OrdinalIgnoreCase))
            {
                var branches = CollectTryBranches(actions, ref index);
                ExecuteTry(remoteDebuggingUrl, automationClient, branches, context, recorder, output);
                continue;
            }

            if (IsConditionalBranch(actions[index].Name) || IsTryBranch(actions[index].Name) || IsSwitchBranch(actions[index].Name))
            {
                var parent = IsTryBranch(actions[index].Name) ? "try" : IsSwitchBranch(actions[index].Name) ? "switch" : "if";
                throw new ScriptActionFailedException($"Line {actions[index].LineNumber}: {actions[index].Name} failed. {actions[index].Name} must follow a {parent} block.");
            }

            ExecuteOneAction(remoteDebuggingUrl, automationClient, actions[index], context, recorder, output, index + 1);
        }
    }

    private static IReadOnlyList<BrowserScriptAction> CollectBranches(IReadOnlyList<BrowserScriptAction> actions, ref int index)
    {
        var branches = new List<BrowserScriptAction> { actions[index] };
        while (index + 1 < actions.Count && IsConditionalBranch(actions[index + 1].Name))
        {
            branches.Add(actions[++index]);
        }

        return branches;
    }

    private static bool IsConditionalBranch(string name) =>
        name.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("else", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<BrowserScriptAction> CollectTryBranches(IReadOnlyList<BrowserScriptAction> actions, ref int index)
    {
        var branches = new List<BrowserScriptAction> { actions[index] };
        while (index + 1 < actions.Count && IsTryBranch(actions[index + 1].Name))
        {
            branches.Add(actions[++index]);
        }

        return branches;
    }

    private static bool IsTryBranch(string name) =>
        name.Equals("catch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("finally", StringComparison.OrdinalIgnoreCase);

    private static bool IsSwitchBranch(string name) =>
        name.Equals("case", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("default", StringComparison.OrdinalIgnoreCase);

    private void ExecuteOneAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction sourceAction,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder,
        List<string> output,
        int stepNumber)
    {
        var action = sourceAction;
        try
        {
            action = ShouldExpandBeforeDispatch(sourceAction.Name) ? ExpandVariables(sourceAction, context) : sourceAction;
            recorder?.BeforeAction(action);
            var stepOutput = ExecuteAction(remoteDebuggingUrl, automationClient, action, context, recorder);
            recorder?.AfterAction(action);
            output.Add($"PASS {stepNumber:000} {action.Name} {FormatActionForLog(action)}".TrimEnd());
            output.AddRange(stepOutput);
        }
        catch (Exception exception) when (exception is ScriptExecutionException or ChromeDevToolsException or ElementNotFoundException)
        {
            throw new ScriptActionFailedException($"Line {action.LineNumber}: {action.Name} failed. {exception.Message}");
        }
    }

    private static bool ShouldExpandBeforeDispatch(string name) =>
        !name.Equals("if", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("elseif", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("while", StringComparison.OrdinalIgnoreCase);
}

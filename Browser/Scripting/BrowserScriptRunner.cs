using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private readonly BrowserScriptParser parser;

    public BrowserScriptRunner(BrowserScriptParser parser)
    {
        this.parser = parser;
    }
    public ScriptRunResult Run(
        string file,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        FileInfo? gif,
        FileInfo? trace = null,
        ScriptTimeoutOptions? timeouts = null,
        string? baseUrl = null,
        IReadOnlyDictionary<string, string>? variables = null,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        string? gifTimelinePath = null)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptRunResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return RunParsedScript(readResult.Script ?? string.Empty, remoteDebuggingUrl, automationClient, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, gifTimelinePath);
    }

    public ScriptRunResult RunText(
        string script,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        FileInfo? gif = null,
        FileInfo? trace = null,
        ScriptTimeoutOptions? timeouts = null,
        string? baseUrl = null,
        IReadOnlyDictionary<string, string>? variables = null,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        string? gifTimelinePath = null)
    {
        return RunParsedScript(script, remoteDebuggingUrl, automationClient, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, gifTimelinePath);
    }

    private ScriptRunResult RunParsedScript(
        string script,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string>? variables,
        GifQuality gifQuality,
        ScriptPointerMotionOptions? pointerMotion,
        ClickPulseStyle clickPulse,
        int holdAfterActionMilliseconds,
        int holdOnFailureMilliseconds,
        string? gifTimelinePath)
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

        string? normalizedBaseUrl;
        try
        {
            normalizedBaseUrl = NormalizeBaseUrl(baseUrl);
        }
        catch (ScriptExecutionException exception)
        {
            return ScriptRunResult.Fail(exception.Message);
        }

        var context = new ScriptExecutionContext
        {
            Trace = trace is null ? null : new BrowserScriptTraceSession(trace.FullName, suppressNested: true),
            DefaultTimeout = timeouts?.DefaultTimeout,
            NavigationTimeout = timeouts?.NavigationTimeout,
            AssertionTimeout = timeouts?.AssertionTimeout,
            BaseUrl = normalizedBaseUrl
        };
        foreach (var variable in variables ?? EmptyVariables)
        {
            context.SetVariable(variable.Key, variable.Value);
        }

        var output = new List<string>();
        using var recorder = gif is null
            ? null
            : new ScriptGifRecorder(automationClient, new ScriptRecordingOptions(gif.FullName, gifQuality, pointerMotion, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, GifTimelinePath.Resolve(gif.FullName, gifTimelinePath)));

        recorder?.Start(remoteDebuggingUrl);

        try
        {
            ExecuteActions(remoteDebuggingUrl, automationClient, parseResult.Actions, context, recorder, output);
            if (context.SoftFailures.Count > 0)
            {
                var error = $"Soft assertion failure(s): {string.Join(" | ", context.SoftFailures)}";
                FinishRecording(recorder, output, failure: true);
                FinishTrace(context, success: false, error, output);
                return ScriptRunResult.Fail(error, output, context.StepRecords);
            }
        }
        catch (ScriptActionFailedException exception)
        {
            FinishRecording(recorder, output, failure: true);
            FinishTrace(context, success: false, exception.Message, output);
            return ScriptRunResult.Fail(exception.Message, output, context.StepRecords);
        }
        catch (LoopControlException exception)
        {
            FinishRecording(recorder, output, failure: true);
            FinishTrace(context, success: false, $"{exception.Kind} must be inside a loop.", output);
            return ScriptRunResult.Fail($"{exception.Kind} must be inside a loop.", output, context.StepRecords);
        }
        catch (ScriptSkipException exception)
        {
            output.Add($"SKIP {exception.LineNumber:000} {exception.Reason}");
            FinishRecording(recorder, output);
            FinishTrace(context, success: true, exception.Reason, output);
            return ScriptRunResult.Skip(exception.Reason, output, context.StepRecords);
        }

        FinishRecording(recorder, output);
        FinishTrace(context, success: true, error: null, output);

        return ScriptRunResult.Ok(output, context.StepRecords);
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
            action = ApplySelectorScope(action, context);
            action = ApplyFrameScope(action, context);
            action = ApplyTimeoutDefaults(action, context);
            recorder?.BeforeAction(action);
            var sequence = context.NextSequence();
            var stepOutput = ExecuteAction(remoteDebuggingUrl, automationClient, action, context, recorder);
            if (ShouldCaptureAfterAction(action))
            {
                recorder?.AfterAction(action);
            }
            var stepLines = new List<string> { FormatStepLine("PASS", sequence, action, context, FormatActionForLog(action)) };
            stepLines.AddRange(FormatPayloadLines(stepOutput, sequence, action, context));
            output.AddRange(stepLines);
            context.StepRecords.Add(new ScriptStepRecord(sequence, action.LineNumber, action.Name, context.CurrentContext, true, stepLines, null));
            context.Trace?.Record(sequence, action, context.CurrentContext, success: true, error: null, stepLines);
        }
        catch (Exception exception) when (exception is ScriptExecutionException or ChromeDevToolsException or ElementNotFoundException)
        {
            var contextText = string.IsNullOrWhiteSpace(context.CurrentContext) ? string.Empty : $" in {context.CurrentContext}";
            var error = $"Line {action.LineNumber}: {action.Name} failed{contextText}. {exception.Message}";
            var sequence = context.NextSequence();
            context.StepRecords.Add(new ScriptStepRecord(sequence, action.LineNumber, action.Name, context.CurrentContext, false, [], error));
            context.Trace?.Record(sequence, action, context.CurrentContext, success: false, error, []);
            throw new ScriptActionFailedException(error);
        }
    }

}

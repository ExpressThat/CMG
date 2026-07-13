using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteTimelineBlock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 0, 0);
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException($"{action.Name} requires a block body.");
        }

        var name = action.Name.ToLowerInvariant();
        if (recorder is null)
        {
            ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder: null, output);
            output.Add($"GIF_TIMELINE_BLOCK {action.LineNumber:000} mode={Mode(name)} status=inactive");
            return;
        }

        if (name is "hidefromgif" or "cutgif")
        {
            recorder.SuspendCapture(() => ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
            output.Add($"GIF_TIMELINE_BLOCK {action.LineNumber:000} mode=cut status=applied");
            return;
        }

        var factor = PlaybackFactor(action);
        var rate = name == "slowdowngif" ? 1d / factor : factor;
        recorder.WithPlaybackRate(rate, () => ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
        output.Add($"GIF_TIMELINE_BLOCK {action.LineNumber:000} mode={Mode(name)} factor={factor:0.###} status=applied");
    }

    private static double PlaybackFactor(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("factor", out var value) ||
            !double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var factor) ||
            factor <= 0 || factor > 100)
        {
            throw new ScriptExecutionException($"{action.Name} option factor= must be greater than zero and at most 100.");
        }

        return factor;
    }

    private static string Mode(string name) => name switch
    {
        "speedupgif" => "speed-up",
        "slowdowngif" => "slow-down",
        _ => "cut"
    };
}

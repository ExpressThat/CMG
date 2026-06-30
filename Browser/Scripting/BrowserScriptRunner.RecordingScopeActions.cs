namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteRecordingScope(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 0, 0);
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException($"{action.Name} requires a block body.");
        }

        ValidateRecordingOptions(action);
        context.PushRecordingDefaults(RecordingDefaultsFrom(action.Options), () =>
            ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
    }

    private static BrowserScriptAction ApplyRecordingDefaults(BrowserScriptAction action, ScriptExecutionContext context)
    {
        var defaults = context.CurrentRecordingDefaults;
        if (defaults.Count is 0)
        {
            return action;
        }

        if (action.Name.Equals("recording", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("withRecording", StringComparison.OrdinalIgnoreCase))
        {
            return action;
        }

        var options = new Dictionary<string, string>(defaults, StringComparer.OrdinalIgnoreCase);
        foreach (var option in action.Options)
        {
            options[option.Key] = option.Value;
        }

        return action with { Options = options };
    }

    private static void ValidateRecordingOptions(BrowserScriptAction action)
    {
        foreach (var option in action.Options)
        {
            if (!RecordingScopeOptions.Contains(option.Key))
            {
                throw new ScriptExecutionException(
                    $"{action.Name} option {option.Key}= is not a supported recording default.");
            }
        }
    }

    private static IReadOnlyDictionary<string, string> RecordingDefaultsFrom(IReadOnlyDictionary<string, string> options) =>
        options
            .Where(option => RecordingScopeOptions.Contains(option.Key))
            .ToDictionary(option => option.Key, option => option.Value, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> RecordingScopeOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "quality",
        "pointerDuration",
        "pointerSpeed",
        "pointerEasing",
        "clickPulse",
        "pulse",
        "holdAfterAction",
        "holdOnFailure",
        "timeline",
        "fps",
        "frameDelay"
    };
}

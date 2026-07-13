using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecutePointerStyle(
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder)
    {
        if (recorder is null) return [SkipLine("GIF_POINTER_STYLE", action)];
        RequireArgumentCount(action, 0, 0);
        if (action.Children.Count > 0) throw new ScriptExecutionException("pointerStyle does not accept a block body.");
        var invalid = action.Options.Keys.FirstOrDefault(option => !PointerStyleOptions.Contains(option));
        if (invalid is not null) throw new ScriptExecutionException($"pointerStyle option {invalid}= is not supported.");
        _ = PointerVisualOptions.FromOptions(action.Options, "pointerStyle");
        if (action.Options.TryGetValue("showPointer", out var visibility))
            _ = PointerVisibilityOptions.Parse(visibility, "pointerStyle option");
        context.SetRecordingDefaults(action.Options);
        return [$"GIF_POINTER_STYLE {action.LineNumber:000} status=updated {FormatRecordingSettings(context.CurrentRecordingDefaults)}"];
    }

    private static IReadOnlyList<string> ExecuteTargetAnnotation(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        if (recorder is null) return [SkipLine("GIF_TARGET_ANNOTATION", action)];
        RequireArgumentCount(action, 1, 2);
        if (action.Children.Count > 0) throw new ScriptExecutionException($"{action.Name} does not accept a block body.");
        var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase);
        if (action.Arguments.Count == 2) options["message"] = action.Arguments[1];
        var highlight = action with { Name = "highlight", Arguments = [action.Arguments[0]], Options = options };
        ExecuteHighlight(remoteDebuggingUrl, automationClient, highlight);
        var duration = GetIntOption(action, "duration", 1200);
        return [$"GIF_TARGET_ANNOTATION {action.LineNumber:000} selector={QuoteField(action.Arguments[0])} duration={duration} status=captured"];
    }

    private static IReadOnlyList<string> ExecuteRecordVariable(
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder)
    {
        if (recorder is null) return [SkipLine("GIF_VARIABLE", action)];
        RequireArgumentCount(action, 1, 1);
        if (action.Children.Count > 0) throw new ScriptExecutionException("recordVariable does not accept a block body.");
        var name = action.Arguments[0];
        if (!context.TryGetVariable(name, out var value)) throw new ScriptExecutionException($"Variable '{name}' is not defined.");
        var reveal = BooleanOption(action, "reveal", false);
        var shown = !reveal && SensitiveVariable(name) ? "[masked]" : value;
        var label = action.Options.GetValueOrDefault("label") ?? name;
        recorder.CaptureVariable(action, $"{label}: {shown}");
        return [$"GIF_VARIABLE {action.LineNumber:000} name={QuoteField(name)} value={QuoteField(shown)} status=captured"];
    }

    private static bool BooleanOption(BrowserScriptAction action, string name, bool fallback)
    {
        if (!action.Options.TryGetValue(name, out var value)) return fallback;
        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{action.Name} option {name}= must be true or false.")
        };
    }

    private static bool SensitiveVariable(string name) =>
        SensitiveNames.Any(value => name.Contains(value, StringComparison.OrdinalIgnoreCase));

    private static string SkipLine(string label, BrowserScriptAction action) =>
        $"{label} {action.LineNumber:000} status=skipped reason=no-active-recording";

    private static readonly HashSet<string> PointerStyleOptions = new(StringComparer.OrdinalIgnoreCase)
        { "pointerTheme", "pointerColor", "pointerSize", "pointerShadow", "showPointer" };
    private static readonly string[] SensitiveNames = ["password", "token", "secret", "authorization", "cookie", "apiKey"];
}

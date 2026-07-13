namespace CMG.Browser.Scripting;

public sealed record RecordingSettingsPreview(bool Success, IReadOnlyList<string> Lines, string? Error = null);

public static class RecordingSettingsPreviewer
{
    public static RecordingSettingsPreview PreviewFile(string file)
    {
        if (!File.Exists(file)) return Fail($"Script file '{file}' was not found.");
        var fullPath = Path.GetFullPath(file);
        var expanded = ScriptImportExpander.Expand(File.ReadAllText(fullPath), Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
        return expanded.Success ? PreviewText(expanded.Script ?? string.Empty) : Fail(expanded.Error ?? "Could not import script.");
    }

    public static RecordingSettingsPreview PreviewText(string script)
    {
        var parsed = new BrowserScriptParser().Parse(script);
        if (!parsed.Success) return Fail(parsed.Error ?? "Could not parse script.");
        var validationError = Validate(parsed.Actions);
        if (validationError is not null) return Fail(validationError);
        var lines = new List<string>();
        Visit(parsed.Actions, new(StringComparer.OrdinalIgnoreCase), lines, "script");
        if (lines.Count == 0) lines.Add("GIF_SETTINGS scope=script options=none");
        return new(true, lines);
    }

    private static string? Validate(IEnumerable<BrowserScriptAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.Name.Equals("recording", StringComparison.OrdinalIgnoreCase) ||
                action.Name.Equals("withRecording", StringComparison.OrdinalIgnoreCase) ||
                action.Name.Equals("recordingDefaults", StringComparison.OrdinalIgnoreCase) ||
                action.Name.Equals("setRecording", StringComparison.OrdinalIgnoreCase))
            {
                var unknown = action.Options.Keys.FirstOrDefault(option => !BrowserScriptRunner.IsRecordingOption(option));
                if (unknown is not null)
                    return $"Line {action.LineNumber}: {action.Name} option {unknown}= is not a supported recording default.";
            }
            var nested = Validate(action.Children);
            if (nested is not null) return nested;
        }
        return null;
    }

    private static void Visit(IReadOnlyList<BrowserScriptAction> actions, Dictionary<string, string> inherited,
        List<string> lines, string scope)
    {
        var current = new Dictionary<string, string>(inherited, StringComparer.OrdinalIgnoreCase);
        foreach (var action in actions)
        {
            var name = action.Name.ToLowerInvariant();
            var own = RecordingOptions(action.Options);
            if (name == "setrecording")
            {
                Merge(current, own);
                lines.Add(Line("setRecording", action, current));
                continue;
            }
            if (name is "recording" or "withrecording" or "recordingdefaults")
            {
                var nested = new Dictionary<string, string>(current, StringComparer.OrdinalIgnoreCase);
                Merge(nested, own);
                lines.Add(Line(name, action, nested));
                Visit(action.Children, nested, lines, $"{scope}/{name}");
                continue;
            }
            if (name is "gif" or "recordvideo" or "screencast" or "gififchanged" or "gif.ifchanged" or "gifonfailure" or "gif.onfailure")
            {
                var effective = new Dictionary<string, string>(current, StringComparer.OrdinalIgnoreCase);
                Merge(effective, own);
                lines.Add(Line(name, action, effective));
            }
            else if (name == "previewrecordingsettings") lines.Add(Line(scope, action, current));
            WarnIgnored(action, own, lines);
            if (action.Children.Count > 0) Visit(action.Children, current, lines, $"{scope}/{name}");
        }
    }

    private static void WarnIgnored(BrowserScriptAction action, IReadOnlyDictionary<string, string> options, List<string> lines)
    {
        if (VisualActions.Contains(action.Name) || options.Count == 0) return;
        foreach (var option in options.Keys.Where(VisualOnlyOptions.Contains))
            lines.Add($"GIF_SETTINGS_WARN line={action.LineNumber} action={action.Name} option={option} reason=non-visual-action");
    }

    private static Dictionary<string, string> RecordingOptions(IReadOnlyDictionary<string, string> options) =>
        options.Where(value => BrowserScriptRunner.IsRecordingOption(value.Key))
            .ToDictionary(value => value.Key, value => value.Value, StringComparer.OrdinalIgnoreCase);

    private static void Merge(IDictionary<string, string> target, IReadOnlyDictionary<string, string> values)
    { foreach (var value in values) target[value.Key] = value.Value; }

    private static string Line(string scope, BrowserScriptAction action, IReadOnlyDictionary<string, string> values) =>
        $"GIF_SETTINGS scope={scope} line={action.LineNumber} action={action.Name} options=" +
        (values.Count == 0 ? "none" : string.Join(',', values.OrderBy(value => value.Key).Select(value => $"{value.Key}={value.Value}")));

    private static RecordingSettingsPreview Fail(string error) => new(false, [], error);

    private static readonly HashSet<string> VisualOnlyOptions = new(StringComparer.OrdinalIgnoreCase)
        { "pointerDuration", "pointerSpeed", "pointerEasing", "pointerPath", "clickPulse", "showPointer", "holdAfterAction" };
    private static readonly HashSet<string> VisualActions = new(StringComparer.OrdinalIgnoreCase)
        { "gif", "recordVideo", "screencast", "gifIfChanged", "gif.ifChanged", "gifOnFailure", "gif.onFailure", "gifSnapshot", "gif.snapshot", "click", "hover", "fill", "type", "dragAndDrop", "moveMouse", "wheel", "scrollTo", "scrollBy", "pauseGif", "caption", "step", "pointerStyle", "annotateTarget", "highlightTarget", "recordVariable" };
}

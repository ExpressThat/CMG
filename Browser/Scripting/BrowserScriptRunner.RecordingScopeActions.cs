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

    private static IReadOnlyList<string> ExecuteSetRecording(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 0, 0);
        if (action.Children.Count > 0) throw new ScriptExecutionException("setRecording does not accept a block body.");
        ValidateRecordingOptions(action);
        var values = RecordingDefaultsFrom(action.Options);
        context.SetRecordingDefaults(values);
        return [$"RECORDING_SETTINGS {action.LineNumber:000} {FormatRecordingSettings(context.CurrentRecordingDefaults)}"];
    }

    private static IReadOnlyList<string> ExecutePreviewRecordingSettings(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 0, 0);
        if (action.Options.Count > 0 || action.Children.Count > 0)
            throw new ScriptExecutionException("previewRecordingSettings does not accept options or a block body.");
        return [$"GIF_SETTINGS {action.LineNumber:000} {FormatRecordingSettings(context.CurrentRecordingDefaults)}"];
    }

    private static BrowserScriptAction ApplyRecordingDefaults(BrowserScriptAction action, ScriptExecutionContext context)
    {
        var defaults = context.CurrentRecordingDefaults;
        if (defaults.Count is 0)
        {
            return action;
        }

        if (action.Name.Equals("recording", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("withRecording", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("recordingDefaults", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("setRecording", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("pointerStyle", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("previewRecordingSettings", StringComparison.OrdinalIgnoreCase))
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
            if (!IsRecordingOption(option.Key))
            {
                throw new ScriptExecutionException(
                    $"{action.Name} option {option.Key}= is not a supported recording default.");
            }
        }
    }

    private static IReadOnlyDictionary<string, string> RecordingDefaultsFrom(IReadOnlyDictionary<string, string> options) =>
        options
            .Where(option => IsRecordingOption(option.Key))
            .ToDictionary(option => option.Key, option => option.Value, StringComparer.OrdinalIgnoreCase);

    private static string FormatRecordingSettings(IReadOnlyDictionary<string, string> values) =>
        values.Count == 0 ? "options=none" : "options=" + string.Join(',', values.OrderBy(value => value.Key, StringComparer.OrdinalIgnoreCase)
            .Select(value => $"{value.Key}={value.Value.Replace(",", "\\,", StringComparison.Ordinal)}"));

    internal static bool IsRecordingOption(string name) => RecordingScopeOptions.Contains(name);

    private static readonly HashSet<string> RecordingScopeOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "quality",
        "dither",
        "palette",
        "colors",
        "background",
        "gradientMode",
        "highContrastPalette",
        "keepFrames",
        "crop",
        "cropPadding",
        "smartCrop",
        "splitTabs",
        "safeArea",
        "layoutStability",
        "viewport",
        "pixelRatio",
        "scale",
        "maxWidth",
        "maxHeight",
        "pointerDuration",
        "pointerSpeed",
        "pointerEasing",
        "pointerTheme",
        "pointerColor",
        "pointerSize",
        "pointerShadow",
        "showPointer",
        "captionStyle",
        "captionPosition",
        "captionSeverity",
        "captionSize",
        "captionFormat",
        "autoCaptions",
        "captionTemplate",
        "captionDuration",
        "captionStacking",
        "persistentStepTitle",
        "sourceLineCaptions",
        "debugNarration",
        "captionPassLabel",
        "captionFailedLabel",
        "captionExpectedLabel",
        "captionActualLabel",
        "compressLongWaits",
        "longWaitThreshold",
        "longWaitDuration",
        "waitProgress",
        "fadeIn",
        "fadeOut",
        "assertionCaptions",
        "failureCaptions",
        "eventCaptions",
        "networkCaptions",
        "dialogCaptions",
        "consoleCaptions",
        "downloadCaptions",
        "uploadCaptions",
        "serviceWorkerCaptions",
        "webSocketCaptions",
        "workerCaptions",
        "redact",
        "redactStyle",
        "redactColor",
        "redactReplacement",
        "redactPadding",
        "autoRedact",
        "redactionSafety",
        "accessibilityEvidence",
        "showKeystrokes",
        "showMouseButtons",
        "focusEvidence",
        "accessibleNames",
        "highContrast",
        "contrastWarnings",
        "reducedMotion",
        "highContrastPointer",
        "debug",
        "debugAction",
        "debugContext",
        "debugTarget",
        "debugCoordinates",
        "debugScroll",
        "intro",
        "outro",
        "introDuration",
        "outroDuration",
        "resultOutro",
        "coalesceDuplicates",
        "sampleEvery",
        "sizeBudget",
        "budgetQualityFallback",
        "budgetDownscaleFallback",
        "pointerContrast",
        "targetCallout",
        "targetCalloutThreshold",
        "targetZoom",
        "targetZoomThreshold",
        "pagePosition",
        "tabContext",
        "focusPulse",
        "pointerIdle",
        "pointerIdleThreshold",
        "teleportMarker",
        "mouseDownHold",
        "pointerPath",
        "dragPath",
        "pressedPointer",
        "dragTrail",
        "dragBreadcrumbs",
        "clickPulse",
        "pulse",
        "holdAfterAction",
        "holdOnFailure",
        "preClickHold",
        "postClickHold",
        "holdAfterMove",
        "holdAfterNavigation",
        "holdAfterAssertion",
        "timeline",
        "fps",
        "frameDelay"
    };
}

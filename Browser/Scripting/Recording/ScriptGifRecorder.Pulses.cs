namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void CapturePulseFrame() => CapturePulseFrame(null);

    private void CapturePulseFrame(BrowserScriptAction? action) =>
        CaptureFrame(FrameDelayCentisecondsFor(action), PulseStyleFor(action), action);

    private void CaptureClickPulseFrames(BrowserScriptAction action)
    {
        var pulseStyle = PulseStyleFor(action);
        var pulseCount = PulseCountFor(action);
        for (var index = 0; index < pulseCount; index++)
        {
            CaptureFrame(FrameDelayCentisecondsFor(action), pulseStyle, action);
        }
    }

    private ClickPulseStyle PulseStyleFor(BrowserScriptAction? action)
    {
        if (action?.Options.TryGetValue("clickPulse", out var value) is true ||
            action?.Options.TryGetValue("pulse", out value) is true)
        {
            return ClickPulseStyleParser.TryParse(value, out var style)
                ? style
                : throw new ScriptExecutionException($"{action.Name} option clickPulse= must be one of: {ClickPulseStyleParser.Values}.");
        }

        if (action is not null)
        {
            if (GifRecordingPresetOptions.Boolean(action.Options, "reducedMotion", false, action.Name))
            {
                return ClickPulseStyle.Dot;
            }
            var actionName = action.Name.ToLowerInvariant();
            if (actionName is "rightclick" or "contextclick" ||
                action.Options.GetValueOrDefault("button")?.Equals("right", StringComparison.OrdinalIgnoreCase) is true)
            {
                return ClickPulseStyle.Crosshair;
            }

            if (action.Options.GetValueOrDefault("button")?.Equals("middle", StringComparison.OrdinalIgnoreCase) is true)
            {
                return ClickPulseStyle.Dot;
            }
        }

        return options.ClickPulse;
    }

    private static int PulseCountFor(BrowserScriptAction action)
    {
        var name = action.Name.ToLowerInvariant();
        if (name is "dblclick" or "doubleclick")
        {
            return 2;
        }

        var countText = action.Options.GetValueOrDefault("clickCount") ?? action.Options.GetValueOrDefault("count");
        return int.TryParse(countText, out var count) && count > 1 ? Math.Min(count, 4) : 1;
    }
}

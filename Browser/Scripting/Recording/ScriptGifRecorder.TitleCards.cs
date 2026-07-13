namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private const int DefaultTitleCardDurationMilliseconds = 1200;
    private bool configuredIntroCaptured;
    private string? configuredOutro;
    private int configuredOutroDuration = DefaultTitleCardDurationMilliseconds;

    private void CaptureConfiguredTitleCards(BrowserScriptAction action)
    {
        var isIntroAction = action.Name.Equals("intro", StringComparison.OrdinalIgnoreCase);
        var isOutroAction = action.Name.Equals("outro", StringComparison.OrdinalIgnoreCase);
        if (isIntroAction)
        {
            configuredIntroCaptured = true;
        }

        if (!isOutroAction && action.Options.TryGetValue("outro", out var outro) && !string.IsNullOrWhiteSpace(outro))
        {
            configuredOutro = outro;
            configuredOutroDuration = DurationOption(action, "outroDuration", DefaultTitleCardDurationMilliseconds);
        }

        if (configuredIntroCaptured ||
            !action.Options.TryGetValue("intro", out var intro) || string.IsNullOrWhiteSpace(intro))
        {
            return;
        }

        configuredIntroCaptured = true;
        CaptureTitleCard(intro, "intro", DurationOption(action, "introDuration", DefaultTitleCardDurationMilliseconds));
    }

    private void CaptureConfiguredOutro()
    {
        if (!string.IsNullOrWhiteSpace(configuredOutro))
        {
            CaptureTitleCard(configuredOutro, "outro", configuredOutroDuration);
        }
    }

    public void CaptureTitleCard(BrowserScriptAction action, string kind)
    {
        var duration = DurationOption(action, "duration", DefaultTitleCardDurationMilliseconds);
        CaptureTitleCard(action.Arguments[0], kind, duration);
    }

    private void CaptureTitleCard(string text, string kind, int durationMilliseconds)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended)
        {
            return;
        }

        devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
        PrimeCropBounds();
        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowTitleCard(text, kind));
        try
        {
            var screenshot = CapturePage(promoteMessageBar: false, allowCachedCrop: true);
            frameSink.AddFrame(screenshot, ScaleDelay(Math.Max(1, (durationMilliseconds + 9) / 10)));
        }
        finally
        {
            devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveTitleCard());
        }
    }

    private static int DurationOption(BrowserScriptAction action, string name, int fallback)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            return fallback;
        }

        var duration = ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option {name}=");
        return duration > 0 ? duration : throw new ScriptExecutionException($"{action.Name} option {name}= must be greater than zero.");
    }
}

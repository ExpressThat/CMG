namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private const int DefaultTitleCardDurationMilliseconds = 1200;
    private bool configuredIntroCaptured;
    private bool explicitOutroCaptured;
    private string? configuredIntro;
    private int configuredIntroDuration = DefaultTitleCardDurationMilliseconds;
    private string? configuredOutro;
    private int configuredOutroDuration = DefaultTitleCardDurationMilliseconds;
    private bool resultOutro;

    private void InitializeTitleCards()
    {
        var defaults = options.EffectiveTitleCards;
        configuredIntro = defaults.Intro;
        configuredIntroDuration = defaults.IntroDuration;
        configuredOutro = defaults.Outro;
        configuredOutroDuration = defaults.OutroDuration;
        resultOutro = defaults.ResultOutro;
    }

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
        if (action.Options.ContainsKey("resultOutro"))
        {
            resultOutro = options.EffectiveTitleCards.WithOptions(action.Options, action.Name).ResultOutro;
        }

        if (action.Options.TryGetValue("intro", out var actionIntro) && !string.IsNullOrWhiteSpace(actionIntro))
        {
            configuredIntro = actionIntro;
            configuredIntroDuration = DurationOption(action, "introDuration", DefaultTitleCardDurationMilliseconds);
        }

        if (configuredIntroCaptured ||
            string.IsNullOrWhiteSpace(configuredIntro))
        {
            return;
        }

        configuredIntroCaptured = true;
        CaptureTitleCard(configuredIntro, "intro", configuredIntroDuration);
    }

    private void CaptureConfiguredOutro(GifRecordingOutcome outcome)
    {
        if (explicitOutroCaptured) return;
        if (!string.IsNullOrWhiteSpace(configuredOutro))
        {
            CaptureTitleCard(configuredOutro, "outro", configuredOutroDuration);
            return;
        }
        if (resultOutro) CaptureTitleCard(ResultText(outcome), "result", configuredOutroDuration);
    }

    public void CaptureTitleCard(BrowserScriptAction action, string kind)
    {
        if (kind.Equals("outro", StringComparison.OrdinalIgnoreCase)) explicitOutroCaptured = true;
        var duration = DurationOption(action, "duration", DefaultTitleCardDurationMilliseconds);
        CaptureTitleCard(action.Arguments[0], kind, duration);
    }

    private static string ResultText(GifRecordingOutcome outcome) => outcome switch
    {
        GifRecordingOutcome.Failed => "Test failed",
        GifRecordingOutcome.Skipped => "Test skipped",
        _ => "Test passed"
    };

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
            var screenshot = CapturePage(promoteMessageBar: false, allowCachedCrop: true, applyRedactions: false);
            var delay = ScaleDelay(Math.Max(1, (durationMilliseconds + 9) / 10));
            AddCapturedFrame(screenshot, delay, $"title-{kind}");
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

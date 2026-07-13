namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private readonly List<string> stepCaptions = [];

    public void EnterStep(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended) return;
        var stacking = BoolOption(action, "captionStacking", true);
        if (!stacking) stepCaptions.Clear();
        stepCaptions.Add(StepText(action));
        ShowStepCaption(action);
    }

    public void ExitStep(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended || stepCaptions.Count == 0) return;
        stepCaptions.RemoveAt(stepCaptions.Count - 1);
        if (stepCaptions.Count > 0) ShowStepCaption(action);
        else devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveMessageBar());
    }

    public void CaptureDebugNarration(BrowserScriptAction action, string message)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended || !BoolOption(action, "debugNarration", false)) return;
        var text = BoolOption(action, "sourceLineCaptions", false)
            ? $"{message} (line {action.LineNumber})" : message;
        devToolsClient.ShowMessageBar(remoteDebuggingUrl, text,
            new BrowserCaptionOptions(CaptionStyle.Compact, CaptionPosition.Top, CaptionSeverity.Info));
        CaptureHoldFrame(Math.Min(300, options.HoldAfterActionMilliseconds));
    }

    private void ShowStepCaption(BrowserScriptAction action)
    {
        var text = string.Join("  >  ", stepCaptions);
        devToolsClient.ShowMessageBar(remoteDebuggingUrl!, text, BrowserCaptionOptions.FromOptions(action.Options, action.Name));
        if (BoolOption(action, "persistentStepTitle", false)) CaptureHoldFrame(200);
    }

    private static string StepText(BrowserScriptAction action) =>
        BoolOption(action, "sourceLineCaptions", false)
            ? $"{action.Arguments[0]} (line {action.LineNumber})" : action.Arguments[0];

}

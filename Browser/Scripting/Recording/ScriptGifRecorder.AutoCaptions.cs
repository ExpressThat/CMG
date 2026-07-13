namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void ApplyAutoCaption(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null || !ScriptAutoCaption.TryCreate(action, activeExecutionContext, out var caption))
        {
            return;
        }

        var captionOptions = BrowserCaptionOptions.FromOptions(action.Options, action.Name);
        devToolsClient.ShowMessageBar(remoteDebuggingUrl, caption.Message, captionOptions);
        if (captionOptions.Position is CaptionPosition.Auto && caption.TargetSelector is not null)
        {
            var selector = ResolveLocator(caption.TargetSelector, action.LineNumber);
            devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.AutoPositionMessageBar(selector));
        }
    }
}

using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteDisabledGifBlock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context)
    {
        var output = new List<string>
        {
            $"GIF_SKIPPED {action.LineNumber:000} status=skipped reason=recording-disabled source={GifRecordingPolicy.DisabledSource}"
        };
        context.PushRecordingDefaults(RecordingDefaultsFrom(action.Options), () =>
        {
            foreach (var child in action.Children)
            {
                var prepared = PrepareActionForDispatch(child, context);
                output.AddRange(ExecuteAction(remoteDebuggingUrl, automationClient, prepared, context, recorder: null));
            }
        });
        return output;
    }
}

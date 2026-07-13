using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteGifRedactionAction(
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        var remove = action.Name.Equals("unmaskGif", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("unredactGif", StringComparison.OrdinalIgnoreCase);
        var label = remove ? "GIF_UNREDACT" : "GIF_REDACT";
        if (recorder is null)
        {
            return [$"{label} {action.LineNumber:000} status=skipped reason=no-active-recording"];
        }
        if (recorder.IsCaptureSuspended)
        {
            return [$"{label} {action.LineNumber:000} status=suppressed reason=timeline-cut"];
        }
        if (action.Children.Count > 0)
        {
            throw new ScriptExecutionException($"{action.Name} does not accept a block body.");
        }

        if (remove)
        {
            recorder.RemoveRedaction(action);
            var target = action.Arguments.Count is 0 ? "all" : QuoteField(action.Arguments[0]);
            return [$"{label} {action.LineNumber:000} target={target} status=active"];
        }

        recorder.AddRedaction(action);
        return [$"{label} {action.LineNumber:000} target={QuoteField(action.Arguments[0])} status=active"];
    }
}

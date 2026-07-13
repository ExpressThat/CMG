using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecutePointerVisibilityAction(
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        var label = action.Name.Equals("hidePointer", StringComparison.OrdinalIgnoreCase)
            ? "GIF_HIDE_POINTER"
            : "GIF_SHOW_POINTER";
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

        RequireArgumentCount(action, 0, 0);
        if (action.Name.Equals("hidePointer", StringComparison.OrdinalIgnoreCase))
        {
            recorder.HidePointer(action);
            return [$"{label} {action.LineNumber:000} status=captured"];
        }

        recorder.ShowPointer(action);
        return [$"{label} {action.LineNumber:000} status=captured"];
    }
}

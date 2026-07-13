using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteTitleCardAction(BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        var kind = action.Name.ToLowerInvariant();
        var label = kind == "intro" ? "GIF_INTRO" : "GIF_OUTRO";
        if (recorder is null)
        {
            return [$"{label} {action.LineNumber:000} status=skipped reason=no-active-recording"];
        }

        if (action.Children.Count > 0)
        {
            throw new ScriptExecutionException($"{action.Name} does not accept a block body.");
        }

        RequireArgumentCount(action, 1, 1);
        recorder.CaptureTitleCard(action, kind);
        return [$"{label} {action.LineNumber:000} status=captured"];
    }
}

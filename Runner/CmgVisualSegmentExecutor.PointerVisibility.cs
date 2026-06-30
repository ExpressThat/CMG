using CMG.Browser;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static bool TryGifPointerVisibilityFor(
        CmgNode action,
        PointerVisibility defaults,
        out PointerVisibility visibility,
        out string? error)
    {
        visibility = defaults;
        error = null;
        if (!action.Options.TryGetValue("showPointer", out var value))
        {
            return true;
        }

        try
        {
            visibility = PointerVisibilityOptions.Parse(value, "gif option");
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }
}

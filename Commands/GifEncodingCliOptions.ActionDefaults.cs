using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal sealed partial record GifEncodingCliOptions
{
    private bool TryActionDefaults(
        ParseResult result,
        RunGifSettings? settings,
        out GifActionDefaults defaults,
        out string? error)
    {
        try
        {
            defaults = GifActionDefaults.FromValues(
                Provided(result, TypingDelay) ? result.GetValue(TypingDelay) : settings?.TypingDelay,
                Provided(result, PostHoverHold) ? result.GetValue(PostHoverHold) : settings?.PostHoverHold,
                "GIF");
            error = null;
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            defaults = new();
            error = exception.Message;
            return false;
        }
    }
}

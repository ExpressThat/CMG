using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static bool TryGifEncodingFor(
        CmgNode action,
        GifEncodingOptions? defaults,
        FileInfo? gif,
        out GifEncodingOptions encoding,
        out string? error)
    {
        error = null;
        try
        {
            encoding = (defaults ?? new GifEncodingOptions())
                .WithOptions(action.Options, "gif", gif?.FullName ?? "recording.gif")
                .ForOutput(gif?.FullName ?? "recording.gif", isolate: defaults?.KeepFramesDirectory is not null && !action.Options.ContainsKey("keepFrames"));
            encoding = encoding with
            {
                Framing = (defaults?.Framing ?? new GifFramingOptions()).WithOptions(action.Options, "gif")
            };
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            encoding = new();
            error = exception.Message;
            return false;
        }
    }
}

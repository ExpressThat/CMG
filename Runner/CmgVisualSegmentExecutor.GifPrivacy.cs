using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static bool TryGifRedactionFor(
        CmgNode action,
        GifRedactionOptions? defaults,
        out GifRedactionOptions redaction,
        out string? error)
    {
        error = null;
        try
        {
            var parsed = GifRedactionOptions.FromOptions(action.Options, "gif option");
            redaction = parsed with
            {
                Rules = action.Options.ContainsKey("redact") ? parsed.EffectiveRules : defaults?.EffectiveRules,
                Auto = action.Options.ContainsKey("autoRedact") ? parsed.Auto : defaults?.Auto ?? parsed.Auto,
                Strict = action.Options.ContainsKey("redactionSafety") ? parsed.Strict : defaults?.Strict ?? parsed.Strict
            };
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            redaction = defaults ?? new();
            error = exception.Message;
            return false;
        }
    }

    private static string FormatQuality(GifQuality quality) =>
        quality.ToString().ToLowerInvariant();

    private static bool IsRecordingBlock(string name) =>
        name.Equals("gif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("recordVideo", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("screencast", StringComparison.OrdinalIgnoreCase);
}

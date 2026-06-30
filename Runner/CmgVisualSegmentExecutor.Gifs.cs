using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static FileInfo? ResolveGifPath(CmgTestCase test, CmgNode action, CmgRunOptions options)
    {
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            return new FileInfo(output);
        }

        var name = action.Arguments.Count > 0 ? action.Arguments[0] : test.Name;
        var safeName = string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        var directory = options.GifDirectory?.FullName ?? Directory.GetCurrentDirectory();
        return new FileInfo(Path.Combine(directory, $"{safeName}.gif"));
    }

    private static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options, int attempt)
    {
        if (options.GifDirectory is null)
        {
            return null;
        }

        var path = CmgRunService.BuildGifPath(test, options);
        if (attempt <= 1 || path is null)
        {
            return path;
        }

        var name = Path.GetFileNameWithoutExtension(path.Name);
        return new FileInfo(Path.Combine(path.DirectoryName ?? string.Empty, $"{name}-attempt-{attempt}.gif"));
    }

    private static bool TryGifQualityFor(CmgNode action, out GifQuality quality, out string? error)
    {
        quality = GifQuality.Highest;
        error = null;
        if (!action.Options.TryGetValue("quality", out var value))
        {
            return true;
        }

        if (GifQualityParser.TryParse(value, out quality))
        {
            return true;
        }

        error = $"gif quality must be one of: {GifQualityParser.Values}.";
        return false;
    }

    private static bool TryGifMotionFor(
        CmgNode action,
        ScriptPointerMotionOptions? defaults,
        out ScriptPointerMotionOptions motion,
        out string? error)
    {
        motion = defaults ?? ScriptPointerMotionOptions.Default;
        error = null;
        try
        {
            motion = motion.WithAction(new CMG.Browser.Scripting.BrowserScriptAction(
                action.LineNumber,
                action.Kind,
                action.Kind,
                action.Arguments,
                action.Options,
                []));
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static bool TryGifPulseFor(
        CmgNode action,
        ClickPulseStyle defaults,
        out ClickPulseStyle pulse,
        out string? error)
    {
        pulse = defaults;
        error = null;
        if (!action.Options.TryGetValue("clickPulse", out var value))
        {
            return true;
        }

        if (ClickPulseStyleParser.TryParse(value, out pulse))
        {
            return true;
        }

        error = $"gif clickPulse must be one of: {ClickPulseStyleParser.Values}.";
        return false;
    }

    private static bool TryGifHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        hold = defaults;
        error = null;
        if (!action.Options.TryGetValue("holdAfterAction", out var value))
        {
            return true;
        }

        try
        {
            hold = ScriptPointerMotionOptions.ParseDuration(value, "gif option holdAfterAction=");
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }
}

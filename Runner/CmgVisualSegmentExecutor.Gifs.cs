using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static FileInfo? ResolveGifPath(CmgTestCase test, CmgNode action, CmgRunOptions options)
    {
        if (GifRecordingPolicy.IsDisabled) return null;
        var format = action.Options.TryGetValue("format", out var requested)
            ? GifArtifactFormatParser.Parse(requested, "gif")
            : options.GifEncoding?.Format ?? GifArtifactFormat.Gif;
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            return new FileInfo(GifArtifactFormatParser.WithExtension(output, format));
        }

        var name = action.Arguments.Count > 0 ? action.Arguments[0] : test.Name;
        var safeName = string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        var directory = options.GifDirectory?.FullName ?? Directory.GetCurrentDirectory();
        return new FileInfo(Path.Combine(directory, $"{safeName}{format.Extension()}"));
    }

    private static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options, int attempt)
    {
        if (options.GifDirectory is null || GifRecordingPolicy.IsDisabled)
        {
            return null;
        }

        var path = CmgRunService.BuildGifPath(test, options);
        if (attempt <= 1 || path is null)
        {
            return path;
        }

        var name = Path.GetFileNameWithoutExtension(path.Name);
        return new FileInfo(Path.Combine(path.DirectoryName ?? string.Empty, $"{name}-attempt-{attempt}{path.Extension}"));
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

    private static bool TryGifVisualFor(
        CmgNode action,
        PointerVisualOptions? defaults,
        out PointerVisualOptions visual,
        out string? error)
    {
        visual = defaults ?? PointerVisualOptions.Default;
        error = null;
        try
        {
            visual = visual.WithAction(new CMG.Browser.Scripting.BrowserScriptAction(
                action.LineNumber,
                action.Kind,
                action.Kind,
                action.Arguments,
                action.Options,
                []), touch: false);
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static bool TryGifCaptionFor(
        CmgNode action,
        BrowserCaptionOptions? defaults,
        out BrowserCaptionOptions? caption,
        out string? error)
    {
        caption = defaults;
        error = null;
        try
        {
            caption = (caption ?? BrowserCaptionOptions.Default).WithAction(new CMG.Browser.Scripting.BrowserScriptAction(
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

    private static bool TryGifHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        return TryGifDurationFor(action, "holdAfterAction", "gif option holdAfterAction=", defaults, out hold, out error);
    }

    private static bool TryGifFailureHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        return TryGifDurationFor(action, "holdOnFailure", "gif option holdOnFailure=", defaults, out hold, out error);
    }

    private static bool TryGifPreClickHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        return TryGifDurationFor(action, "preClickHold", "gif option preClickHold=", defaults, out hold, out error);
    }

    private static bool TryGifPostClickHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        return TryGifDurationFor(action, "postClickHold", "gif option postClickHold=", defaults, out hold, out error);
    }

    private static bool TryGifNavigationHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        return TryGifDurationFor(action, "holdAfterNavigation", "gif option holdAfterNavigation=", defaults, out hold, out error);
    }

    private static bool TryGifAssertionHoldFor(CmgNode action, int defaults, out int hold, out string? error)
    {
        return TryGifDurationFor(action, "holdAfterAssertion", "gif option holdAfterAssertion=", defaults, out hold, out error);
    }

    private static bool TryGifTimelineFor(CmgNode action, FileInfo? gif, CmgRunOptions options, out string? timeline, out string? error)
    {
        error = null;
        timeline = GifTimelineFor(gif, action.Options.GetValueOrDefault("timeline") ?? options.GifTimelinePath);
        return true;
    }

    private static bool TryGifFrameDelayFor(CmgNode action, int defaults, out int frameDelay, out string? error)
    {
        frameDelay = defaults;
        error = null;
        try
        {
            frameDelay = ScriptFrameTimingOptions.FromOptions(action.Options, action.Kind, defaults);
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static string? GifTimelineFor(FileInfo? gif, string? requestedPath) =>
        gif is null ? null : GifTimelinePath.Resolve(gif.FullName, requestedPath);

    private static bool TryGifDurationFor(CmgNode action, string option, string source, int defaults, out int hold, out string? error)
    {
        hold = defaults;
        error = null;
        if (!action.Options.TryGetValue(option, out var value))
        {
            return true;
        }

        try
        {
            hold = ScriptPointerMotionOptions.ParseDuration(value, source);
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }
}

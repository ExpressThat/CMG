using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    internal static bool TryApplyDeclaredGifDefaults(
        CmgTestCase test,
        CmgRunOptions source,
        out CmgRunOptions effective,
        out string? error)
    {
        effective = source;
        error = null;
        var mapped = MapGifOptions(test.Options);
        try
        {
            if (!CmgGifRetentionPolicy.TryParse(test, source, out _, out error)) return false;
            var quality = source.GifQuality;
            if (test.Options.TryGetValue("gifQuality", out var rawQuality) &&
                !GifQualityParser.TryParse(rawQuality, out quality))
                throw new ScriptExecutionException($"test option gifQuality= must be one of: {GifQualityParser.Values}.");

            var action = new BrowserScriptAction(0, "test", "test", [], mapped, []);
            var motion = (source.PointerMotion ?? ScriptPointerMotionOptions.Default).WithAction(action).Validate("test");
            var framing = (source.GifEncoding?.Framing ?? new GifFramingOptions()).WithOptions(mapped, "test");
            var pointerEvidence = (source.GifEncoding?.PointerEvidence ?? new GifPointerEvidenceOptions()).WithOptions(mapped, "test");
            var encoding = (source.GifEncoding ?? new GifEncodingOptions()) with { Framing = framing, PointerEvidence = pointerEvidence };
            var frameDelay = ScriptFrameTimingOptions.FromOptions(mapped, "test", source.FrameDelayMilliseconds);
            effective = source with
            {
                GifQuality = quality,
                PointerMotion = motion,
                GifEncoding = encoding,
                FrameDelayMilliseconds = frameDelay
            };
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static Dictionary<string, string> MapGifOptions(IReadOnlyDictionary<string, string> options)
    {
        var mapped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(options, mapped, "gifPointerDuration", "pointerDuration");
        Add(options, mapped, "gifPointerSpeed", "pointerSpeed");
        Add(options, mapped, "gifPointerEasing", "pointerEasing");
        Add(options, mapped, "gifPointerPath", "pointerPath");
        Add(options, mapped, "gifDragPath", "dragPath");
        Add(options, mapped, "gifFps", "fps");
        Add(options, mapped, "gifFrameDelay", "frameDelay");
        Add(options, mapped, "gifCrop", "crop");
        Add(options, mapped, "gifCropPadding", "cropPadding");
        Add(options, mapped, "gifSmartCrop", "smartCrop");
        Add(options, mapped, "gifSplitTabs", "splitTabs");
        Add(options, mapped, "gifScale", "scale");
        Add(options, mapped, "gifMaxWidth", "maxWidth");
        Add(options, mapped, "gifMaxHeight", "maxHeight");
        Add(options, mapped, "gifViewport", "viewport");
        Add(options, mapped, "gifPixelRatio", "pixelRatio");
        Add(options, mapped, "gifSafeArea", "safeArea");
        Add(options, mapped, "gifLayoutStability", "layoutStability");
        Add(options, mapped, "gifTargetZoom", "targetZoom");
        Add(options, mapped, "gifTargetZoomThreshold", "targetZoomThreshold");
        Add(options, mapped, "gifPagePosition", "pagePosition");
        Add(options, mapped, "gifTabContext", "tabContext");
        return mapped;
    }

    private static void Add(
        IReadOnlyDictionary<string, string> source,
        IDictionary<string, string> target,
        string declaration,
        string recording)
    {
        if (source.TryGetValue(declaration, out var value)) target[recording] = value;
    }
}

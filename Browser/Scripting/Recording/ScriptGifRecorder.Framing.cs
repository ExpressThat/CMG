namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private byte[] CapturePage(bool promoteMessageBar, bool allowCachedCrop = false, bool applyRedactions = true)
    {
        if (remoteDebuggingUrl is null) return [];
        try
        {
            if (applyRedactions) PrepareRedactions();
            if (applyRedactions) PreparePointerEvidence();
            if (applyRedactions) PrepareAccessibilityEvidence();
            if (applyRedactions) PrepareDebugEvidence();
            var framing = options.EffectiveFraming;
            if (framing.SmartCropWidth is not null)
                return devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar,
                    options: new ScreenshotOptions(Clip: ResolveSmartCropClip(framing)));
            if (framing.CropSelector is null)
            {
                if (framing.PixelRatio == 1d)
                    return devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar);
                var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl);
                var offset = ResolveViewportOffset();
                return devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar,
                    options: new ScreenshotOptions(Clip: new ScreenshotClip(offset.X, offset.Y, viewport.Width, viewport.Height, framing.PixelRatio)));
            }

            var clip = allowCachedCrop && lastCropClip is not null
                ? lastCropClip
                : ResolveCropClip(framing);

            return devToolsClient.GetPageScreenshot(
                remoteDebuggingUrl,
                promoteMessageBar,
                options: new ScreenshotOptions(Clip: clip));
        }
        finally
        {
            if (applyRedactions) RemoveDebugEvidence();
            if (applyRedactions) RemoveAccessibilityEvidence();
            if (applyRedactions) RemovePointerEvidence();
            if (applyRedactions) RemoveRedactionOverlays();
        }
    }

    private ElementPoint ResolveViewportOffset()
    {
        var json = devToolsClient.Evaluate(remoteDebuggingUrl!, "JSON.stringify({x:scrollX,y:scrollY})");
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            using var nested = System.Text.Json.JsonDocument.Parse(root.GetString() ?? "{}");
            return OffsetFrom(nested.RootElement);
        }
        return OffsetFrom(root);
    }

    private static ElementPoint OffsetFrom(System.Text.Json.JsonElement root) => new(
        root.TryGetProperty("x", out var x) ? x.GetDouble() : 0,
        root.TryGetProperty("y", out var y) ? y.GetDouble() : 0);

    private ScreenshotClip ResolveCropClip(GifFramingOptions framing)
    {
        var selector = ResolveLocator(framing.CropSelector!, 0);
        var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl!);
        var box = devToolsClient.GetElementBox(remoteDebuggingUrl!, selector);
        if (box.X + box.Width <= 0 || box.Y + box.Height <= 0 || box.X >= viewport.Width || box.Y >= viewport.Height)
        {
            devToolsClient.ScrollElementIntoView(remoteDebuggingUrl!, selector);
            StabilizeTarget(selector);
            box = devToolsClient.GetElementBox(remoteDebuggingUrl!, selector);
        }
        var padding = Math.Max(framing.CropPadding, framing.SafeArea);
        var localX = Math.Max(0, box.X - padding);
        var localY = Math.Max(0, box.Y - padding);
        var width = Math.Min(viewport.Width - localX, box.Width + padding * 2);
        var height = Math.Min(viewport.Height - localY, box.Height + padding * 2);
        if (width <= 0 || height <= 0)
            throw new ScriptExecutionException($"GIF crop selector '{framing.CropSelector}' resolved outside the viewport.");
        var offset = ResolveViewportOffset();
        return lastCropClip = new ScreenshotClip(offset.X + localX, offset.Y + localY, width, height, framing.PixelRatio);
    }

    private ScreenshotClip ResolveSmartCropClip(GifFramingOptions framing)
    {
        var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl!);
        var width = Math.Min(viewport.Width, framing.SmartCropWidth!.Value);
        var height = Math.Min(viewport.Height, framing.SmartCropHeight!.Value);
        var center = pointer.Position;
        var action = frameAction ?? activeAction;
        var selector = action is null ? null : AccessibilityTarget(action);
        if (selector is not null)
        {
            try
            {
                var box = devToolsClient.GetElementBox(remoteDebuggingUrl!, selector);
                center = new ElementPoint((center.X + box.X + box.Width / 2) / 2, (center.Y + box.Y + box.Height / 2) / 2);
            }
            catch (ChromeDevToolsException) { }
        }
        var localX = Math.Clamp(center.X - width / 2, 0, viewport.Width - width);
        var localY = Math.Clamp(center.Y - height / 2, 0, viewport.Height - height);
        var offset = ResolveViewportOffset();
        return new ScreenshotClip(offset.X + localX, offset.Y + localY, width, height, framing.PixelRatio);
    }

    private void PrimeCropBounds()
    {
        var framing = options.EffectiveFraming;
        if (framing.CropSelector is not null && lastCropClip is null) ResolveCropClip(framing);
    }
}

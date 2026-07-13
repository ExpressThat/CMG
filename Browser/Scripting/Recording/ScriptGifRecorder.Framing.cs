namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private byte[] CapturePage(bool promoteMessageBar, bool allowCachedCrop = false)
    {
        if (remoteDebuggingUrl is null) return [];
        var framing = options.EffectiveFraming;
        if (framing.CropSelector is null)
            return devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar);

        var clip = allowCachedCrop && lastCropClip is not null
            ? lastCropClip
            : ResolveCropClip(framing);

        return devToolsClient.GetPageScreenshot(
            remoteDebuggingUrl,
            promoteMessageBar,
            options: new ScreenshotOptions(Clip: clip));
    }

    private ScreenshotClip ResolveCropClip(GifFramingOptions framing)
    {
        var selector = ResolveLocator(framing.CropSelector!, 0);
        var box = devToolsClient.GetElementBox(remoteDebuggingUrl!, selector);
        var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl!);
        var padding = framing.CropPadding;
        var x = Math.Max(0, box.X - padding);
        var y = Math.Max(0, box.Y - padding);
        var width = Math.Min(viewport.Width - x, box.Width + padding * 2);
        var height = Math.Min(viewport.Height - y, box.Height + padding * 2);
        if (width <= 0 || height <= 0)
            throw new ScriptExecutionException($"GIF crop selector '{framing.CropSelector}' resolved outside the viewport.");
        return lastCropClip = new ScreenshotClip(x, y, width, height);
    }

    private void PrimeCropBounds()
    {
        var framing = options.EffectiveFraming;
        if (framing.CropSelector is not null && lastCropClip is null) ResolveCropClip(framing);
    }
}

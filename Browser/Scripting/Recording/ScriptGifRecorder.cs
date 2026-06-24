namespace CMG.Browser.Scripting.Recording;

public sealed class ScriptGifRecorder : IDisposable
{
    private readonly IBrowserAutomationClient devToolsClient;
    private readonly ScriptRecordingOptions options;
    private readonly GifFrameSink frameSink = new();
    private readonly VirtualPointer pointer = new();
    private string? remoteDebuggingUrl;

    public ScriptGifRecorder(
        IBrowserAutomationClient devToolsClient,
        ScriptRecordingOptions options)
    {
        this.devToolsClient = devToolsClient;
        this.options = options;
    }

    public string OutputPath => Path.GetFullPath(options.OutputPath);

    public void Start(string remoteDebuggingUrl)
    {
        this.remoteDebuggingUrl = remoteDebuggingUrl;
    }

    public void BeforeAction(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var name = action.Name.ToLowerInvariant();

        if (name is "click" or "type" or "clear" or "hover" or "select")
        {
            if (action.Arguments.Count > 0)
            {
                MoveToSelector(action.Arguments[0]);
            }
        }
        else if (name is "draganddrop" && action.Arguments.Count >= 2)
        {
            MoveToSelector(action.Arguments[0]);
        }
    }

    public void AfterAction(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var name = action.Name.ToLowerInvariant();

        if (name is "click")
        {
            CapturePulseFrame();
            CaptureHoldFrame();
            return;
        }

        if (name is "set")
        {
            return;
        }

        CaptureHoldFrame();
    }

    public void CaptureTypingFrame()
    {
        CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
    }

    public void CaptureClickPulse()
    {
        CapturePulseFrame();
    }

    public void BeginDrag(string sourceSelector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveToSelector(sourceSelector);
        CaptureHoldFrame();
        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
    }

    public void DragHover(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveDragToSelector(selector);
    }

    public void DragDelay(int milliseconds)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var frameDelayMilliseconds = ScriptRecordingOptions.FrameDelayCentiseconds * 10;
        var frameCount = Math.Max(1, milliseconds / Math.Max(1, frameDelayMilliseconds));

        for (var index = 0; index < frameCount; index++)
        {
            devToolsClient.MovePageDrag(remoteDebuggingUrl, pointer.Position);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        }
    }

    public void DropDrag(string targetSelector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveDragToSelector(targetSelector);
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame();
        CaptureHoldFrame();
    }

    public void RecordDragAndDrop(string sourceSelector, string targetSelector, Action drop)
    {
        if (remoteDebuggingUrl is null)
        {
            drop();
            return;
        }

        MoveToSelector(sourceSelector);
        CaptureHoldFrame();

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, targetSelector);
        var path = pointer.MoveTo(target, ScriptRecordingOptions.MovementFrameCount).ToArray();

        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);

        devToolsClient.MouseDragAndDrop(remoteDebuggingUrl, sourceSelector, targetSelector, path, point =>
        {
            pointer.Set(point);
            devToolsClient.MovePageDrag(remoteDebuggingUrl, point);
            devToolsClient.MoveDomCursor(remoteDebuggingUrl, point);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        });

        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        drop();
        CapturePulseFrame();
        CaptureHoldFrame();
    }

    public void Finish()
    {
        if (remoteDebuggingUrl is not null)
        {
            TryRemoveDomCursor();
        }

        frameSink.Save(OutputPath);
    }

    public void Dispose()
    {
        frameSink.Dispose();
    }

    private void MoveToSelector(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        foreach (var point in pointer.MoveTo(target, ScriptRecordingOptions.MovementFrameCount))
        {
            devToolsClient.MoveDomCursor(remoteDebuggingUrl, point);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        }
    }

    private void MoveDragToSelector(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        foreach (var point in pointer.MoveTo(target, ScriptRecordingOptions.MovementFrameCount))
        {
            devToolsClient.MovePageDrag(remoteDebuggingUrl, point);
            devToolsClient.MoveDomCursor(remoteDebuggingUrl, point);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        }
    }

    private void CaptureHoldFrame()
    {
        CaptureFrame(ScriptRecordingOptions.HoldFrameDelayCentiseconds);
    }

    private void CapturePulseFrame()
    {
        CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds, pulse: true);
    }

    private void CaptureFrame(int delayCentiseconds, bool pulse = false)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        devToolsClient.PromoteMessageBar(remoteDebuggingUrl);
        devToolsClient.MoveDomCursor(remoteDebuggingUrl, pointer.Position);
        var screenshot = devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar: false);
        frameSink.AddFrame(screenshot, delayCentiseconds);
    }

    private void TryRemoveDomCursor()
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        try
        {
            devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
            devToolsClient.RemoveDefaultDragGhost(remoteDebuggingUrl);
        }
        catch (ChromeDevToolsException)
        {
        }
    }
}

namespace CMG.Browser.Scripting.Recording;

public sealed class ScriptGifRecorder : IDisposable
{
    private static readonly string[] MouseAliases =
    [
        "center",
        "top",
        "bottom",
        "left",
        "right",
        "topLeft",
        "topRight",
        "bottomLeft",
        "bottomRight"
    ];

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
            devToolsClient.MoveMouse(remoteDebuggingUrl, pointer.Position, buttons: 1);
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

    public void MoveMouse(BrowserScriptAction action, bool dragging)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MovePointerTo(ResolveMoveMouseTarget(action), dragging);
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

        MovePointerTo(target, dragging: false);
    }

    private void MoveDragToSelector(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        MovePointerTo(target, dragging: true);
    }

    private void MovePointerTo(ElementPoint target, bool dragging)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        foreach (var point in pointer.MoveTo(target, ScriptRecordingOptions.MovementFrameCount))
        {
            devToolsClient.MoveMouse(remoteDebuggingUrl, point, dragging ? 1 : 0);
            if (dragging)
            {
                devToolsClient.MovePageDrag(remoteDebuggingUrl, point);
            }

            devToolsClient.MoveDomCursor(remoteDebuggingUrl, point);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        }
    }

    private ElementPoint ResolveMoveMouseTarget(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return pointer.Position;
        }

        var hasAlias = action.Arguments.Count is 1;
        var hasCoordinates = action.Options.ContainsKey("x") || action.Options.ContainsKey("y");

        if (hasAlias == hasCoordinates)
        {
            throw new ScriptExecutionException("moveMouse requires either one alias argument or x=<pixels> y=<pixels> options.");
        }

        var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl);
        var target = hasAlias
            ? ResolveAlias(action.Arguments[0], viewport)
            : new ElementPoint(ParseCoordinate(action, "x"), ParseCoordinate(action, "y"));

        if (target.X < 0 || target.Y < 0 || target.X > viewport.Width || target.Y > viewport.Height)
        {
            throw new ScriptExecutionException(
                $"moveMouse target ({FormatNumber(target.X)}, {FormatNumber(target.Y)}) is outside the current viewport {FormatNumber(viewport.Width)}x{FormatNumber(viewport.Height)}.");
        }

        return target;
    }

    private static ElementPoint ResolveAlias(string alias, ViewportSize viewport)
    {
        var inset = Math.Min(16, Math.Max(0, Math.Min(viewport.Width, viewport.Height) / 2));
        var centerX = viewport.Width / 2;
        var centerY = viewport.Height / 2;
        var right = Math.Max(0, viewport.Width - inset);
        var bottom = Math.Max(0, viewport.Height - inset);

        return alias.ToLowerInvariant() switch
        {
            "center" => new ElementPoint(centerX, centerY),
            "top" => new ElementPoint(centerX, inset),
            "bottom" => new ElementPoint(centerX, bottom),
            "left" => new ElementPoint(inset, centerY),
            "right" => new ElementPoint(right, centerY),
            "topleft" => new ElementPoint(inset, inset),
            "topright" => new ElementPoint(right, inset),
            "bottomleft" => new ElementPoint(inset, bottom),
            "bottomright" => new ElementPoint(right, bottom),
            _ => throw new ScriptExecutionException($"Unknown moveMouse alias '{alias}'. Supported aliases: {string.Join(", ", MouseAliases)}.")
        };
    }

    private static double ParseCoordinate(BrowserScriptAction action, string name)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            throw new ScriptExecutionException($"Missing required option '{name}'.");
        }

        if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) || number < 0)
        {
            throw new ScriptExecutionException($"moveMouse option '{name}' must be a non-negative number.");
        }

        return number;
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

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

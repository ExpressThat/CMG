namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder : IDisposable
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
    private readonly GifFrameSink frameSink;
    private readonly List<GifTimelineCheckpoint> checkpoints = [];
    private readonly VirtualPointer pointer = new();
    private string? remoteDebuggingUrl;
    private bool cursorPressed;
    private bool cursorTrail;
    private bool cursorBreadcrumb;
    private PointerVisualOptions cursorVisual = PointerVisualOptions.Default;

    public ScriptGifRecorder(
        IBrowserAutomationClient devToolsClient,
        ScriptRecordingOptions options)
    {
        this.devToolsClient = devToolsClient;
        this.options = options;
        frameSink = new GifFrameSink(options.Quality);
    }

    public string OutputPath => Path.GetFullPath(options.OutputPath);

    public IReadOnlyList<GifTimelineCheckpoint> Checkpoints => checkpoints;

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

        if (name is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" or "download" or "type" or
            "presssequentially" or "fill" or "clear" or "hover" or "select" or "selectoption" or "check" or "uncheck" or "focus" or "blur" or
            "selecttext" or "highlight" or "uploadfiles" or "setinputfiles" or "selectfile" or "expectscreenshot" or "tohavescreenshot")
        {
            if (action.Arguments.Count > 0)
            {
                MoveToSelector(action);
            }

            if (IsClickAction(name))
            {
                CapturePreClickHoldFrame(action);
            }
        }
        else if ((name is "draganddrop" or "dragto") && action.Arguments.Count >= 2)
        {
            MoveToSelector(action);
        }
        else if (name is "frameclick" or "frametype" or "framefill" or "framehover" && action.Arguments.Count >= 2)
        {
            MoveToFrameSelector(action);
            if (IsClickAction(name))
            {
                CapturePreClickHoldFrame(action);
            }
        }
    }

    public void AfterAction(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var name = action.Name.ToLowerInvariant();

        if (name is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" or "download" or "frameclick")
        {
            CaptureClickPulseFrames(action);
            CapturePostClickHoldFrame(action);
            return;
        }

        if (IsNavigationAction(name))
        {
            CaptureNavigationHoldFrame(action);
            return;
        }

        if (IsAssertionAction(name))
        {
            CaptureAssertionHoldFrame(action);
            return;
        }

        if (name is "set")
        {
            return;
        }

        CaptureHoldFrame(action);
    }

    public void CaptureTypingFrame()
    {
        CaptureFrame(options.FrameDelayCentiseconds);
    }

    public void CaptureClickPulse()
    {
        CapturePulseFrame();
    }

    public void RecordCheckpoint(BrowserScriptAction action, string name)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        checkpoints.Add(new GifTimelineCheckpoint(
            name,
            action.LineNumber,
            frameSink.FrameCount,
            frameSink.DurationMilliseconds));
    }

    private static bool IsClickAction(string name) =>
        name is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" or "download" or "frameclick";

    private static bool IsNavigationAction(string name) =>
        name is "navigate" or "goto" or "visit" or "reload" or "goback" or "goforward" or
            "waitforurl" or "waitfortitle" or "expecturl" or "expecttitle" or "tohaveurl" or "tohavetitle" or
            "waitforloadstate" or "waitfornetworkidle" or "networkidle" or "waitfornavigation";

    private static bool IsAssertionAction(string name) =>
        name.StartsWith("expect", StringComparison.Ordinal) ||
        name.StartsWith("assert", StringComparison.Ordinal) ||
        name.StartsWith("tohave", StringComparison.Ordinal) ||
        name.StartsWith("tobe", StringComparison.Ordinal) ||
        name.StartsWith("tonot", StringComparison.Ordinal) ||
        name is "contains" or "notcontains" or "unchecked";
}

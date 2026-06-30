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
    private readonly VirtualPointer pointer = new();
    private string? remoteDebuggingUrl;

    public ScriptGifRecorder(
        IBrowserAutomationClient devToolsClient,
        ScriptRecordingOptions options)
    {
        this.devToolsClient = devToolsClient;
        this.options = options;
        frameSink = new GifFrameSink(options.Quality);
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

        if (name is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" or "download" or "type" or
            "presssequentially" or "fill" or "clear" or "hover" or "select" or "selectoption" or "check" or "uncheck" or "focus" or "blur" or
            "selecttext" or "highlight" or "uploadfiles" or "setinputfiles" or "selectfile" or "expectscreenshot" or "tohavescreenshot")
        {
            if (action.Arguments.Count > 0)
            {
                MoveToSelector(action);
            }
        }
        else if ((name is "draganddrop" or "dragto") && action.Arguments.Count >= 2)
        {
            MoveToSelector(action);
        }
        else if (name is "frameclick" or "frametype" or "framefill" or "framehover" && action.Arguments.Count >= 2)
        {
            MoveToFrameSelector(action);
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
            CapturePulseFrame(action);
            CaptureHoldFrame(action);
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
        CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
    }

    public void CaptureClickPulse()
    {
        CapturePulseFrame();
    }
}

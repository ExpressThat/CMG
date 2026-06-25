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

        if (name is "click" or "dblclick" or "rightclick" or "download" or "type" or "fill" or "clear" or
            "hover" or "select" or "selectoption" or "check" or "uncheck" or "focus" or "blur" or "selecttext")
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
        else if (name is "frameclick" or "frametype" or "framefill" or "framehover" && action.Arguments.Count >= 2)
        {
            MoveToFrameSelector(action.Arguments[0], action.Arguments[1]);
        }
    }

    public void AfterAction(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var name = action.Name.ToLowerInvariant();

        if (name is "click" or "dblclick" or "rightclick" or "download" or "frameclick")
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
}

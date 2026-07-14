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
    private readonly List<GifTimelineStep> timelineSteps = [];
    private readonly Dictionary<int, GifTimelineStepStart> activeTimelineSteps = [];
    private readonly List<GifRedactionRule> redactions;
    private readonly List<GifRedactionRule> actionRedactions = [];
    private GifAutoRedactionMode activeAutoRedaction;
    private bool activeStrictRedaction;
    private readonly List<GifRedactionAuditEntry> redactionAudit = [];
    private readonly List<GifDebugFrame> debugFrames = [];
    private readonly VirtualPointer pointer = new();
    private string? remoteDebuggingUrl;
    private bool cursorPressed;
    private bool cursorTrail;
    private bool cursorBreadcrumb;
    private bool redactionCaptureBlocked;
    private PointerVisualOptions cursorVisual = PointerVisualOptions.Default;
    private int captureSuspensionDepth;
    private double playbackRate = 1d;
    private ScreenshotClip? lastCropClip;
    private BrowserScriptAction? activeAction;
    private string activeExecutionContext = string.Empty;

    public ScriptGifRecorder(
        IBrowserAutomationClient devToolsClient,
        ScriptRecordingOptions options)
    {
        this.devToolsClient = devToolsClient;
        this.options = options;
        redactions = [.. options.EffectiveRedaction.EffectiveRules];
        activeAutoRedaction = options.EffectiveRedaction.Auto;
        activeStrictRedaction = options.EffectiveRedaction.Strict;
        frameSink = new GifFrameSink(options.Quality, options.EffectiveEncoding, options.EffectiveFraming);
        InitializeTitleCards();
    }

    public string OutputPath => Path.GetFullPath(options.OutputPath);

    public IReadOnlyList<GifTimelineCheckpoint> Checkpoints => checkpoints;

    public string? RetainedFramesDirectory => options.EffectiveEncoding.KeepFramesDirectory;

    public int FrameCount => frameSink.FrameCount;

    public void BeforeAction(BrowserScriptAction action, int? sequence = null, string context = "")
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended)
        {
            return;
        }

        if (sequence is int stepSequence)
        {
            activeTimelineSteps[stepSequence] = new GifTimelineStepStart(
                stepSequence,
                action.LineNumber,
                action.Name,
                context,
                frameSink.FrameCount,
                frameSink.DurationMilliseconds);
        }
        ConfigureActionRedactions(action);
        activeAction = action;
        activeExecutionContext = context;
        var name = action.Name.ToLowerInvariant();
        CaptureWaitAction(action, name);
        CaptureConfiguredTitleCards(action);
        ApplyAutoCaption(action);

        if (name is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" or "download" or "type" or
            "presssequentially" or "fill" or "clear" or "hover" or "select" or "selectoption" or "check" or "uncheck" or "focus" or "blur" or
            "selecttext" or "highlight" or "annotatetarget" or "highlighttarget" or "uploadfiles" or "setinputfiles" or "selectfile" or "expectscreenshot" or "tohavescreenshot")
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

    public void AfterAction(BrowserScriptAction action, IReadOnlyList<string>? actionOutput = null)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended)
        {
            return;
        }

        var name = action.Name.ToLowerInvariant();
        var eventCaption = ShowEventCaption(action, actionOutput ?? []);
        captureAfterAction = true;

        try
        {
            if (name is "caption" or "showmessagebar" && CaptureCaptionTimeline(action))
            {
                return;
            }

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
                if (!eventCaption) CaptureAssertionCaption(action, actionOutput ?? []);
                CaptureAssertionHoldFrame(action);
                return;
            }

            if (name is "set")
            {
                return;
            }

            if (name is "mousedown")
            {
                CaptureHoldFrame(PointerEvidenceFor(action).MouseDownHoldMilliseconds, action);
                return;
            }

            CaptureHoldFrame(action);
        }
        finally
        {
            captureAfterAction = false;
            if (eventCaption) RemoveEventCaption();
        }
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
        if (remoteDebuggingUrl is null || IsCaptureSuspended)
        {
            return;
        }

        checkpoints.Add(new GifTimelineCheckpoint(
            name,
            action.LineNumber,
            frameSink.FrameCount,
            frameSink.DurationMilliseconds));
    }

    public void CompleteAction(int sequence, bool success, string? error = null)
    {
        if (!activeTimelineSteps.Remove(sequence, out var start))
        {
            return;
        }

        int? endFrame = frameSink.DurationMilliseconds > start.StartTimeMilliseconds && frameSink.FrameCount > 0
            ? frameSink.FrameCount - 1 : null;
        var startFrame = endFrame is int retainedEnd ? Math.Min(start.StartFrameIndex, retainedEnd) : start.StartFrameIndex;
        timelineSteps.Add(new GifTimelineStep(
            start.Sequence,
            start.LineNumber,
            start.Action,
            start.Context,
            success,
            startFrame,
            endFrame,
            start.StartTimeMilliseconds,
            frameSink.DurationMilliseconds,
            success ? null : endFrame,
            error));
        RecordNonVisualWarning(start);
        actionRedactions.Clear();
        activeAutoRedaction = options.EffectiveRedaction.Auto;
        activeStrictRedaction = options.EffectiveRedaction.Strict;
        activeAction = null;
        activeExecutionContext = string.Empty;
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

using System.CommandLine;
using CMG.Browser;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunCommandBuilderGifConfigTests
{
    [Fact]
    public void RunCommand_MergesGifSettingsIntoCompleteRunOptions()
    {
        using var directory = new TempDirectory();
        var config = directory.Write("""
        {
          "gifSettings": {
            "quality": "medium", "pointerDuration": 700, "pointerSpeed": "slow",
            "pointerEasing": "linear", "pointerPath": "avoid-target", "clickPulse": "dot", "fps": 8,
            "crop": "#panel", "cropPadding": 16, "scale": 0.9,
            "maxWidth": 900, "maxHeight": 700, "viewport": "800x600", "pixelRatio": 1, "safeArea": 20,
            "targetZoom": "none", "targetZoomThreshold": 20, "pagePosition": "always", "tabContext": "always",
            "captionStyle": "teaching", "captionPosition": "top",
            "captionSeverity": "warning", "captionSize": "large",
            "autoCaptions": true, "captionTemplate": "{step}: {action}",
            "redact": ["#root-secret"], "blur": [".token"],
            "autoRedact": "sensitive", "redactionSafety": "strict",
            "stillPdf": "reviews/visual.pdf", "format": "webp", "ffmpegPath": "tools/ffmpeg"
          },
          "projects": [{
            "name": "visual", "gifSettings": {
              "quality": "highest", "pointerSpeed": "fast", "dragPath": "arc", "frameDelay": 120,
              "cropPadding": 24, "layoutStability": 350, "captionPosition": "bottom", "mask": ["#project-secret"]
            }
          }]
        }
        """);
        var service = new CapturingRunService();

        var exit = BuildRoot(service).Parse(
            $"run flows --config \"{config}\" --project visual --pointer-duration 250 --gif-scale 0.75 --gif-safe-area 40 --caption-style qa --gif-redact #cli-secret --gif-auto-redact none").Invoke();

        Assert.Equal(0, exit);
        var options = Assert.IsType<CmgRunOptions>(service.Options);
        Assert.Equal(BrowserKind.Chrome, options.BrowserKind);
        Assert.Equal(CMG.Browser.Scripting.Recording.GifQuality.Highest, options.GifQuality);
        Assert.Equal(250, options.PointerMotion?.PointerDurationMilliseconds);
        Assert.Equal("fast", options.PointerMotion?.PointerSpeed);
        Assert.Equal(CMG.Browser.Scripting.Recording.ScriptPointerPath.AvoidTarget, options.PointerMotion?.PointerPath);
        Assert.Equal(CMG.Browser.Scripting.Recording.ScriptPointerPath.Arc, options.PointerMotion?.DragPath);
        Assert.Equal(120, options.FrameDelayMilliseconds);
        Assert.Equal(CaptionStyle.Qa, options.CaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Bottom, options.CaptionOptions?.Position);
        Assert.Equal(CaptionSeverity.Warning, options.CaptionOptions?.Severity);
        Assert.Equal(CaptionSize.Large, options.CaptionOptions?.Size);
        Assert.True(options.CaptionOptions?.AutoCaptions);
        Assert.Equal("{step}: {action}", options.CaptionOptions?.CaptionTemplate);
        var framing = Assert.IsType<CMG.Browser.Scripting.Recording.GifFramingOptions>(options.GifEncoding?.Framing);
        Assert.Equal("#panel", framing.CropSelector);
        Assert.Equal(24, framing.CropPadding);
        Assert.Equal(0.75, framing.Scale);
        Assert.Equal(900, framing.MaxWidth);
        Assert.Equal(700, framing.MaxHeight);
        Assert.Equal(800, framing.ViewportWidth);
        Assert.Equal(600, framing.ViewportHeight);
        Assert.Equal(1, framing.PixelRatio);
        Assert.Equal(40, framing.SafeArea);
        Assert.Equal(350, framing.LayoutStabilityMilliseconds);
        var evidence = Assert.IsType<CMG.Browser.Scripting.Recording.GifPointerEvidenceOptions>(options.GifEncoding?.PointerEvidence);
        Assert.Equal(CMG.Browser.Scripting.Recording.PointerTargetCalloutMode.None, evidence.TargetZoom);
        Assert.Equal(20, evidence.TargetZoomThreshold);
        Assert.Equal(CMG.Browser.Scripting.Recording.PointerTargetCalloutMode.Always, evidence.PagePosition);
        Assert.Equal(CMG.Browser.Scripting.Recording.PointerTargetCalloutMode.Always, evidence.TabContext);
        var redaction = Assert.IsType<CMG.Browser.Scripting.Recording.GifRedactionOptions>(options.GifEncoding?.Redaction);
        Assert.Equal(CMG.Browser.Scripting.Recording.GifAutoRedactionMode.None, redaction.Auto);
        Assert.True(redaction.Strict);
        Assert.Equal(["#cli-secret", "#project-secret", ".token"], redaction.EffectiveRules.Select(rule => rule.Locator).ToArray());
        Assert.Equal(CMG.Browser.Scripting.Recording.GifRedactionStyle.Blur, redaction.EffectiveRules[2].Style);
        Assert.Equal(CMG.Browser.Scripting.Recording.GifArtifactFormat.Webp, options.GifEncoding?.Format);
        Assert.Equal("reviews/visual.pdf", options.GifEncoding?.Review?.StillPdf);
        Assert.Equal("tools/ffmpeg", options.GifEncoding?.FfmpegPath);
    }

    [Fact]
    public void RunCommand_RejectsInvalidGifSettingsTypeBeforeServiceCall()
    {
        using var directory = new TempDirectory();
        var config = directory.Write("""{ "gifSettings": { "scale": "large" } }""");
        var service = new CapturingRunService();

        var exit = BuildRoot(service).Parse($"run flows --config \"{config}\"").Invoke();

        Assert.Equal(1, exit);
        Assert.Null(service.Options);
    }

    [Fact]
    public void RunCommand_AcceptsSmartCropConfig()
    {
        using var directory = new TempDirectory();
        var config = directory.Write("""{ "gifSettings": { "smartCrop": "500x320" } }""");
        var service = new CapturingRunService();

        Assert.Equal(0, BuildRoot(service).Parse($"run flows --config \"{config}\"").Invoke());
        Assert.Equal((500, 320), (service.Options?.GifEncoding?.Framing?.SmartCropWidth, service.Options?.GifEncoding?.Framing?.SmartCropHeight));
    }

    [Fact]
    public void RunCommand_AcceptsSplitTabsConfig()
    {
        using var directory = new TempDirectory();
        var config = directory.Write("""{ "gifSettings": { "splitTabs": "auto" } }""");
        var service = new CapturingRunService();

        Assert.Equal(0, BuildRoot(service).Parse($"run flows --config \"{config}\"").Invoke());
        Assert.Equal(CMG.Browser.Scripting.Recording.PointerTargetCalloutMode.Auto, service.Options?.GifEncoding?.Framing?.SplitTabs);
    }

    [Fact]
    public void RunCommand_RejectsUnknownGifSettingBeforeServiceCall()
    {
        using var directory = new TempDirectory();
        var config = directory.Write("""{ "gifSettings": { "pointerSpeeed": "fast" } }""");
        var service = new CapturingRunService();

        var exit = BuildRoot(service).Parse($"run flows --config \"{config}\"").Invoke();

        Assert.Equal(1, exit);
        Assert.Null(service.Options);
    }

    private static RootCommand BuildRoot(ICmgRunService service)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome); root.Options.Add(edge); root.Options.Add(firefox);
        root.Subcommands.Add(new RunCommandBuilder(new CmgRunCommandHandler(service))
            .Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class CapturingRunService : ICmgRunService
    {
        public CmgRunOptions? Options { get; private set; }
        public CmgRunResult Run(string path, CmgRunOptions options)
        {
            Options = options;
            return new CmgRunResult(true, [], [], null);
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public string Write(string content)
        {
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, "cmg.run.json");
            File.WriteAllText(path, content);
            return path;
        }
        public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}

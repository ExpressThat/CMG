using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgDeclaredGifDefaultsTests
{
    [Fact]
    public void Apply_MapsTestGifDefaultsOntoRunOptions()
    {
        var test = Test(new Dictionary<string, string>
        {
            ["gifQuality"] = "medium",
            ["gifPointerSpeed"] = "fast",
            ["gifPointerPath"] = "avoid-target",
            ["gifDragPath"] = "arc",
            ["gifFps"] = "20",
            ["gifCrop"] = "#panel",
            ["gifSafeArea"] = "40",
            ["gifLayoutStability"] = "350",
            ["gifScale"] = "0.75"
        });

        var success = CmgVisualSegmentExecutor.TryApplyDeclaredGifDefaults(test, Options(), out var result, out var error);

        Assert.True(success, error);
        Assert.Equal(GifQuality.Medium, result.GifQuality);
        Assert.Equal("fast", result.PointerMotion?.PointerSpeed);
        Assert.Equal(ScriptPointerPath.AvoidTarget, result.PointerMotion?.PointerPath);
        Assert.Equal(ScriptPointerPath.Arc, result.PointerMotion?.DragPath);
        Assert.Equal(50, result.FrameDelayMilliseconds);
        Assert.Equal("#panel", result.GifEncoding?.Framing?.CropSelector);
        Assert.Equal(40, result.GifEncoding?.Framing?.SafeArea);
        Assert.Equal(350, result.GifEncoding?.Framing?.LayoutStabilityMilliseconds);
        Assert.Equal(.75, result.GifEncoding?.Framing?.Scale);
    }

    [Fact]
    public void Apply_TestDefaultsOverrideExistingCliDefaultsPropertyByProperty()
    {
        var source = Options() with
        {
            GifQuality = GifQuality.High,
            GifEncoding = new GifEncodingOptions(Framing: new GifFramingOptions(MaxWidth: 900))
        };

        var success = CmgVisualSegmentExecutor.TryApplyDeclaredGifDefaults(
            Test(new Dictionary<string, string> { ["gifCrop"] = "#dialog" }), source, out var result, out var error);

        Assert.True(success, error);
        Assert.Equal(GifQuality.High, result.GifQuality);
        Assert.Equal("#dialog", result.GifEncoding?.Framing?.CropSelector);
        Assert.Equal(900, result.GifEncoding?.Framing?.MaxWidth);
    }

    [Theory]
    [InlineData("gifQuality", "ultra", "gifQuality=")]
    [InlineData("gifFps", "0", "fps=")]
    [InlineData("gifScale", "2", "scale=")]
    [InlineData("gifPointerPath", "wobble", "pointerPath=")]
    public void Apply_RejectsInvalidDeclaration(string name, string value, string expected)
    {
        var success = CmgVisualSegmentExecutor.TryApplyDeclaredGifDefaults(
            Test(new Dictionary<string, string> { [name] = value }), Options(), out _, out var error);

        Assert.False(success);
        Assert.Contains(expected, error, StringComparison.Ordinal);
    }

    [Fact]
    public void Planner_InheritsSuiteGifDefaultsAndLetsTestOverrideThem()
    {
        var suite = new CmgNode(1, "describe", "visual suite", [], new Dictionary<string, string>
        {
            ["gifQuality"] = "high",
            ["gifFps"] = "12"
        }, [
            new CmgNode(2, "test", "inherits", [], new Dictionary<string, string>(), []),
            new CmgNode(3, "test", "overrides", [], new Dictionary<string, string> { ["gifQuality"] = "low" }, [])
        ]);

        var tests = new CmgTestPlanner().Plan(new CmgDocument("suite.cmgscript", [suite]));

        Assert.Equal("high", tests[0].Options["gifQuality"]);
        Assert.Equal("12", tests[0].Options["gifFps"]);
        Assert.Equal("low", tests[1].Options["gifQuality"]);
        Assert.Equal("12", tests[1].Options["gifFps"]);
    }

    private static CmgTestCase Test(IReadOnlyDictionary<string, string> options) => new("test.cmgscript", "visual", [], options);

    private static CmgRunOptions Options() => new(
        BrowserKind.Chrome, new DirectoryInfo(Path.GetTempPath()), null, null, null, null,
        null, null, 0, 0, 1, false, 1, 1, null, null, null, null, new Dictionary<string, string>());
}

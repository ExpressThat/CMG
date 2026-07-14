using System.Text.Json;
using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserRecordingGeometryTests
{
    [Fact]
    public void ElementRect_UsesHitTestedTransformAwareInteractionPoint()
    {
        var script = BrowserDomScripts.ElementRect("#rotated");

        Assert.Contains("getBoxQuads", script, StringComparison.Ordinal);
        Assert.Contains("document.elementFromPoint", script, StringComparison.Ordinal);
        Assert.Contains("interactionX", script, StringComparison.Ordinal);
        Assert.Contains("currentCSSZoom", script, StringComparison.Ordinal);
        Assert.Contains("visualViewport", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Stabilization_CorrectsNestedScrollableAncestors()
    {
        var script = BrowserDomScripts.StabilizeGifTarget("#nested", 24, 300);

        Assert.Contains("ancestors.push(node)", script, StringComparison.Ordinal);
        Assert.Contains("ancestor.scrollLeft += dx", script, StringComparison.Ordinal);
        Assert.Contains("ancestor.scrollTop += dy", script, StringComparison.Ordinal);
        Assert.Contains("requestAnimationFrame", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Recording_ReportsZoomCalibrationAndWritesTimeline()
    {
        var artifact = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var timeline = Path.ChangeExtension(artifact, ".timeline.json");
        var client = new FakeAutomationClient
        {
            Geometry = new BrowserGeometryMetrics(1.25, 1.1, 2, 3, 4)
        };
        try
        {
            var script = $"gif calibrated output=\"{Slash(artifact)}\" timeline=true {{ pauseGif 20 }}";
            var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText(script, "debug", client);

            Assert.True(result.Success, result.Error);
            Assert.Contains(result.StdoutLines, line => line.StartsWith("GIF_CAPTURE_GEOMETRY", StringComparison.Ordinal) &&
                line.Contains("pageZoom=1.25", StringComparison.Ordinal) &&
                line.Contains("correction=css-pixel-preserving", StringComparison.Ordinal));
            using var document = JsonDocument.Parse(File.ReadAllText(timeline));
            var geometry = document.RootElement.GetProperty("captureDiagnostics").GetProperty("geometry");
            Assert.Equal("css-viewport", geometry.GetProperty("coordinateSpace").GetString());
            Assert.Equal(2, geometry.GetProperty("devicePixelRatio").GetDouble());
        }
        finally
        {
            if (File.Exists(artifact)) File.Delete(artifact);
            if (File.Exists(timeline)) File.Delete(timeline);
        }
    }

    private static string Slash(string path) => path.Replace("\\", "/", StringComparison.Ordinal);
}

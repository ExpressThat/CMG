using CMG.Browser;
using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class CmgVisualAssertionRunnerTests
{
    [Fact]
    public void Run_PageScreenshotPassesFullPageOption()
    {
        using var files = new TempFiles();
        var client = new FakeAutomationClient();
        var action = Node([], new() { ["baseline"] = files.Baseline, ["output"] = files.Actual, ["fullPage"] = "true" });
        WritePng(files.Baseline, Color.White);

        var result = new CmgVisualAssertionRunner().Run(action, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.True(client.LastFullPageScreenshot);
    }

    [Fact]
    public void Run_AppliesMaskBeforeComparison()
    {
        using var files = new TempFiles();
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(0, 0, 1, 1));
        WritePng(files.Baseline, Color.Magenta);

        var action = Node([], new() { ["baseline"] = files.Baseline, ["output"] = files.Actual, ["mask"] = "#clock" });
        var result = new CmgVisualAssertionRunner().Run(action, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#clock", client.LastElementBoxSelector);
    }

    [Fact]
    public void Run_RichLocatorMaskRunsPrefixExpression()
    {
        using var files = new TempFiles();
        var client = new FakeAutomationClient();
        WritePng(files.Baseline, Color.Magenta);

        var action = Node([], new() { ["baseline"] = files.Baseline, ["output"] = files.Actual, ["mask"] = "text=Loading" });
        var result = new CmgVisualAssertionRunner().Run(action, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("text=Loading", client.EvaluatedExpressions[0]);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_4\"]", client.LastElementBoxSelector);
    }

    [Fact]
    public void Run_InvalidFullPageOptionFailsWithReason()
    {
        var action = Node([], new() { ["fullPage"] = "maybe" });

        var result = new CmgVisualAssertionRunner().Run(action, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("fullPage= must be true or false", result.Error);
    }

    private static CmgNode Node(IReadOnlyList<string> args, Dictionary<string, string> options) =>
        new(4, "expectScreenshot", "expectScreenshot", args, options, []);

    private static void WritePng(string path, Color color)
    {
        using var image = new Image<Rgba32>(1, 1, color);
        image.SaveAsPng(path);
    }

    private sealed class TempFiles : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public string Baseline => Path.Combine(directory, "baseline.png");
        public string Actual => Path.Combine(directory, "actual.png");
        public TempFiles() => Directory.CreateDirectory(directory);
        public void Dispose() => Directory.Delete(directory, recursive: true);
    }
}

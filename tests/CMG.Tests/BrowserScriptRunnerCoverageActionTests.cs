using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerCoverageActionTests
{
    [Fact]
    public void RunText_StartCoveragePassesOptions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("startCoverage js=true css=false", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(new CoverageOptions(true, false), client.LastCoverageOptions);
        Assert.Contains("COVERAGE_STARTED 001 js=true css=false", result.StdoutLines);
    }

    [Fact]
    public void RunText_StopCoverageWritesJsonToFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var result = Runner().RunText($"stopCoverage path=\"{Slash(path)}\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Equal("""{"js":[],"css":[]}""", File.ReadAllText(path));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("COVERAGE 001", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_StartCoverageRejectsInvalidBoolean()
    {
        var result = Runner().RunText("startCoverage js=maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("must be true or false", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static string Slash(string path) => path.Replace('\\', '/');
}

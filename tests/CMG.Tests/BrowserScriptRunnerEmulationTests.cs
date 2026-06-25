using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerEmulationTests
{
    [Fact]
    public void RunText_EmulateSetsViewportAndPageOverrides()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            "emulate width=390 height=844 userAgent=\"CMG Mobile\" locale=en-GB colorScheme=dark reducedMotion=reduce timezone=Europe/London geolocation=\"51.5,-0.1\" permissions=geolocation",
            "debug",
            client);

        Assert.True(result.Success);
        Assert.Equal(new(390, 844), client.LastViewport);
        Assert.Contains("userAgent", client.LastExpression);
        Assert.Contains("matchMedia", client.LastExpression);
        Assert.Contains("geolocation", client.LastExpression);
        Assert.Contains("permissions.query", client.LastExpression);
    }

    [Fact]
    public void RunText_EmulateRequiresCompleteViewport()
    {
        var result = Runner().RunText("emulate width=390", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("both width and height", result.Error);
    }

    [Fact]
    public void Build_RejectsInvalidGeolocation()
    {
        var options = new Dictionary<string, string> { ["geolocation"] = "north" };

        Assert.Throws<ScriptExecutionException>(() => BrowserEmulationScript.Build(options));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

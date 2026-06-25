using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserClockScriptsTests
{
    [Fact]
    public void Install_PatchesDateAndTimers()
    {
        var script = BrowserClockScripts.Install(1000);

        Assert.Contains("window.Date = FakeDate", script);
        Assert.Contains("window.setTimeout", script);
        Assert.Contains("1000", script);
    }

    [Fact]
    public void Tick_RunsDueTimersAndAdvancesCurrentTime()
    {
        var script = BrowserClockScripts.Tick(250);

        Assert.Contains("clock.timers", script);
        Assert.Contains("clock.current = end", script);
        Assert.Contains("250", script);
    }

    [Fact]
    public void Restore_ReinstallsOriginalClock()
    {
        var script = BrowserClockScripts.Restore();

        Assert.Contains("clock.original.Date", script);
        Assert.Contains("CLOCK", script, StringComparison.OrdinalIgnoreCase);
    }
}

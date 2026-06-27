using System.Diagnostics;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserFixtureIsolationE2eTests
{
    [Fact]
    public void BrowserFixture_DisposeClosesBrowserAndRemovesWorkspace()
    {
        string localAppData;
        int processId;

        using (var fixture = new CmgBrowserFixture())
        {
            localAppData = fixture.LocalAppData;
            processId = fixture.BrowserProcessId;

            var title = fixture.Cli.Run("browser", "control", "navigation", "title");
            title.ShouldPass();
            title.StdoutContains("CMG E2E Fixture");
            Assert.True(IsProcessRunning(processId), $"Expected browser process {processId} to be running.");
        }

        Assert.False(IsProcessRunning(processId), $"Expected browser process {processId} to be stopped.");
        Assert.False(Directory.Exists(localAppData), $"Expected fixture workspace to be removed: {localAppData}");
    }

    private static bool IsProcessRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

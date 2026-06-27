using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerAccessibilityTests
{
    [Fact]
    public void RunText_AccessibilitySnapshotOutputsJsonLine()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("accessibilitySnapshot \"#root\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("#root", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("ACCESSIBILITY", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_AccessibilitySnapshotCanWriteFile()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var result = Runner().RunText($"accessibilitySnapshot output=\"{file.Replace('\\', '/')}\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.True(File.Exists(file));
    }

    [Fact]
    public void RunText_ExpectAccessibleRequiresRole()
    {
        var result = Runner().RunText("expectAccessible name=Save", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("requires role", result.Error);
    }

    [Fact]
    public void RunText_ExpectAccessibleOutputsMatchLine()
    {
        var result = Runner().RunText("expectAccessible role=button name=Save", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("ACCESSIBLE", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

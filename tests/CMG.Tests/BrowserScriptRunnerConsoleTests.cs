using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerConsoleTests
{
    [Fact]
    public void RunText_CaptureConsoleInstallsHook()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("captureConsole", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgConsole", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONSOLE_CAPTURE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForConsoleUsesExpectedTextAndLevel()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForConsole \"saved\" level=info timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("saved", client.LastExpression);
        Assert.Contains("info", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONSOLE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForConsoleRejectsInvalidLevel()
    {
        var result = Runner().RunText("waitForConsole \"saved\" level=verbose", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("level= must be log, info, warn, or error", result.Error);
    }

    [Fact]
    public void RunText_WaitForConsoleSupportsRegexAndIgnoreCase()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForConsole \"SAVE[D]\" match=regex ignoreCase=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const matchMode = \"regex\";", client.LastExpression);
        Assert.Contains("const ignoreCase = true;", client.LastExpression);
    }

    [Fact]
    public void RunText_WaitForConsoleRejectsInvalidRegex()
    {
        var result = Runner().RunText("waitForConsole \"[\" match=regex", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Invalid text regex '['", result.Error);
    }

    [Fact]
    public void RunText_ExpectNoConsoleUsesDefaultErrorLevel()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectNoConsole timeout=100", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const level = \"error\";", client.LastExpression);
        Assert.Contains("Unexpected console", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONSOLE_OK", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ToHaveNoConsoleUsesTextAndLevelFilter()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("toHaveNoConsole \"deprecated\" level=warn timeout=50", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("deprecated", client.LastExpression);
        Assert.Contains("const level = \"warn\";", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("level=warn", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ToHaveNoConsoleSupportsExactMatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("toHaveNoConsole \"deprecated\" match=exact", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const matchMode = \"exact\";", client.LastExpression);
    }

    [Fact]
    public void RunText_ExpectNoConsoleRejectsInvalidLevel()
    {
        var result = Runner().RunText("expectNoConsole level=verbose", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("level= must be log, info, warn, or error", result.Error);
    }

    [Fact]
    public void RunText_ExpectNoPageErrorUsesCapturedPageErrors()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectNoPageError timeout=100", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgPageErrors", client.LastExpression);
        Assert.Contains("Unexpected page", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("PAGE_ERROR_OK", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ToHaveNoPageErrorUsesTextFilter()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("toHaveNoPageError \"Cannot read\" timeout=50", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Cannot read", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("PAGE_ERROR_OK", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ToHaveNoPageErrorSupportsRegexAndIgnoreCase()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("toHaveNoPageError \"cannot read\" match=regex ignoreCase=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const matchMode = \"regex\";", client.LastExpression);
        Assert.Contains("const ignoreCase = true;", client.LastExpression);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

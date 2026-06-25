using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerFileActionTests
{
    [Fact]
    public void RunText_ReadFileStoresVariableForLaterExpansion()
    {
        var file = TempFile("hello");
        var result = Runner().RunText($"readFile greeting path=\"{Slash(file)}\"\nwriteFile path=\"{Slash(TempPath())}\" text=\"${{greeting}} world\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("FILE_READ", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_FixtureCanReadBase64()
    {
        var file = TempFile("hello");
        var result = Runner().RunText($"fixture blob path=\"{Slash(file)}\" encoding=base64", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("FILE_READ", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_WriteAppendAndExpectFile()
    {
        var path = TempPath();
        var result = Runner().RunText($"""
        writeFile path="{Slash(path)}" text="one"
        appendFile path="{Slash(path)}" text=" two"
        expectFile path="{Slash(path)}" contains="one two"
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Equal("one two", File.ReadAllText(path));
        Assert.Contains(result.StdoutLines, line => line.Contains("FILE_OK", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ExpectFileReportsMissingFile()
    {
        var result = Runner().RunText("expectFile path=\"missing.txt\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected file", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static string TempFile(string text)
    {
        var path = TempPath();
        File.WriteAllText(path, text);
        return path;
    }

    private static string TempPath() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");

    private static string Slash(string path) => path.Replace('\\', '/');
}

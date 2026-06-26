using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptValidatorTests
{
    [Fact]
    public void ValidateText_AcceptsWeirdFormatting()
    {
        var result = Validator().ValidateText("""
                    macro     show       message        {
                              if       (  "ok"   ==   "ok"   )       {
                                        caption          "${message}"
                              }       else       {
                                        fail             "wrong branch"
                              }
                    }

                    call        show        "Ready"
        """);

        Assert.True(result.Success, result.Error);
        Assert.Equal(2, result.ActionCount);
    }

    [Fact]
    public void ValidateFile_ExpandsImportsRelativeToFile()
    {
        using var directory = new TempScriptDirectory();
        directory.Write("shared.cmgscript", """
        macro announce value {
          caption "${value}"
        }
        """);
        var main = directory.Write("flows/main.cmgscript", """
        import "../shared.cmgscript"
        call announce "Imported"
        """);

        var result = Validator().ValidateFile(main);

        Assert.True(result.Success, result.Error);
        Assert.Equal(2, result.ActionCount);
    }

    [Fact]
    public void ValidateFile_ReportsMissingImports()
    {
        using var directory = new TempScriptDirectory();
        var main = directory.Write("main.cmgscript", "import \"missing.cmgscript\"");

        var result = Validator().ValidateFile(main);

        Assert.False(result.Success);
        Assert.Contains("Imported script", result.Error);
        Assert.Contains("was not found", result.Error);
    }

    [Fact]
    public void ValidateText_ReportsMissingBlockClose()
    {
        var result = Validator().ValidateText("""
        if true {
          caption "open"
        """);

        Assert.False(result.Success);
        Assert.Equal("Line 2: missing block close '}'.", result.Error);
    }

    private static BrowserScriptValidator Validator() => new(new BrowserScriptParser());

    private sealed class TempScriptDirectory : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public string Write(string relativePath, string content)
        {
            var fullPath = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content);
            return fullPath;
        }

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}

using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgDslFormattingRobustnessTests
{
    [Fact]
    public void Parse_AllowsSemicolonSeparatedRunnerActionsOutsideQuotes()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        test "semicolons" { caption "one;still one"; if true { caption "two"; caption "three" }; caption "four" }
        """);

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(result.Document!.Nodes);
        Assert.Equal(["caption", "if", "caption"], test.Children.Select(node => node.Kind.ToLowerInvariant()));
        Assert.Equal("one;still one", test.Children[0].Arguments[0]);
        Assert.Equal(["caption", "caption"], test.Children[1].Children.Select(node => node.Kind.ToLowerInvariant()));
    }

    [Fact]
    public void Parse_ImportsWithTabWhitespaceAfterKeyword()
    {
        using var directory = new TempScriptDirectory();
        directory.Write("shared.cmgscript", """
        macro announce {
          caption "Imported"
        }
        """);
        var main = directory.Write("flows/main.cmgscript", """
        import	"../shared.cmgscript"
        test "imports" { call announce }
        """);

        var result = new CmgDslParser().Parse(main, File.ReadAllText(main));

        Assert.True(result.Success, result.Error);
        Assert.Equal("macro", result.Document!.Nodes[0].Kind);
        Assert.Equal("test", result.Document.Nodes[1].Kind);
    }

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

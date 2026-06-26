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

    [Fact]
    public void Parse_ImportsFromSemicolonSeparatedRunnerLine()
    {
        using var directory = new TempScriptDirectory();
        directory.Write("shared.cmgscript", """
        macro announce {
          caption "Imported"
        }
        """);
        var main = directory.Write("flows/main.cmgscript", """
        import "../shared.cmgscript"; test "imports" { call announce; caption "done; still one" }
        """);

        var result = new CmgDslParser().Parse(main, File.ReadAllText(main));

        Assert.True(result.Success, result.Error);
        Assert.Equal("macro", result.Document!.Nodes[0].Kind);
        var test = result.Document.Nodes[1];
        Assert.Equal("test", test.Kind);
        Assert.Equal(["call", "caption"], test.Children.Select(node => node.Kind.ToLowerInvariant()));
        Assert.Equal("done; still one", test.Children[1].Arguments[0]);
    }

    [Fact]
    public void Parse_PostConditionLoopsAllowOddSpacingAndCasing()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        test "loops" {
              UNTIL      ( ${ready} == true )       max=3       { caption "waiting" }
          doWhile    ( ${again} == true )    max=2 { caption "again" }
         doUntil (evaluate "window.ready" == "true") { caption "ready" }
        }
        """);

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(result.Document!.Nodes);
        Assert.Equal(["UNTIL", "doWhile", "doUntil"], test.Children.Select(node => node.Kind));
        Assert.Equal("3", test.Children[0].Options["max"]);
        Assert.Equal("2", test.Children[1].Options["max"]);
    }

    [Fact]
    public void Parse_ToPassAllowsOddSpacingAndInlineBlock()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        test "to pass" {      toPass        max=3        delay=10       {        expectText       "#status"        "Saved"       }       }
        """);

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(result.Document!.Nodes);
        var block = Assert.Single(test.Children);
        Assert.Equal("toPass", block.Kind);
        Assert.Equal("3", block.Options["max"]);
        Assert.Equal("10", block.Options["delay"]);
        Assert.Equal("expectText", Assert.Single(block.Children).Kind);
    }

    [Fact]
    public void Parse_AllowsDenseBranchTryAndLoopFormatting()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        test "dense" { if true { macro inner value { if ( "${value}" != "" ) { caption "${value}" } } ; call inner "ok" } elseif false { fail "bad" } else { caption "else" }; try { repeat 2 { caption "tick" } } catch error { caption "${error}" } finally { caption "done" } }
        """);

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(result.Document!.Nodes);
        Assert.Equal(["if", "elseif", "else", "try", "catch", "finally"], test.Children.Select(node => node.Kind.ToLowerInvariant()));
        Assert.Equal("macro", test.Children[0].Children[0].Kind);
        Assert.Equal("repeat", Assert.Single(test.Children[3].Children).Kind);
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

using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgDslParserTests
{
    [Fact]
    public void Parse_ReadsSuiteTestAndSteps()
    {
        var parser = new CmgDslParser();
        var result = parser.Parse("flow.cmgscript", """
        suite "Visual flow" {
          test "opens dialog" {
            step "Open" {
              click "#open"
            }
          }
        }
        """);

        Assert.True(result.Success);
        var suite = Assert.Single(result.Document!.Nodes);
        Assert.Equal("suite", suite.Kind);
        Assert.Equal("Visual flow", suite.Name);
        Assert.Equal("test", Assert.Single(suite.Children).Kind);
    }

    [Fact]
    public void Parse_ReportsMissingBlockClose()
    {
        var parser = new CmgDslParser();
        var result = parser.Parse("bad.cmgscript", """
        test "broken" {
          click "#open"
        """);

        Assert.False(result.Success);
        Assert.Contains("missing block close", result.Error);
    }

    [Fact]
    public void Parse_PreservesOptionsAndEscapes()
    {
        var parser = new CmgDslParser();
        var result = parser.Parse("flow.cmgscript", """
        test "typing" {
          type "#name" "Line\nTwo" timeout=5000 header.Authorization="Bearer token"
        }
        """);

        var action = Assert.Single(Assert.Single(result.Document!.Nodes).Children);
        Assert.Equal("Line\nTwo", action.Arguments[1]);
        Assert.Equal("5000", action.Options["timeout"]);
        Assert.Equal("Bearer token", action.Options["header.Authorization"]);
    }

    [Fact]
    public void Parse_PreservesEmptyQuotedStrings()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        test "empty" {
          if "" == "" {
            evaluate "true"
          }
        }
        """);

        Assert.True(result.Success);
        var action = Assert.Single(Assert.Single(result.Document!.Nodes).Children);
        Assert.Equal("", action.Arguments[0]);
        Assert.Equal("", action.Arguments[2]);
    }

    [Fact]
    public void Parse_RejectsFlatV1Scripts()
    {
        var result = new CmgDslParser().Parse("old.cmgscript", """
        navigate "https://example.com"
        click "#open"
        """);

        Assert.False(result.Success);
        Assert.Contains("V1 flat scripts are not supported", result.Error);
    }

    [Fact]
    public void Parse_ImportsMacrosRelativeToSourceFile()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "shared.cmg"), """
            macro helper target {
              click "${target}"
            }
            """);

            var result = new CmgDslParser().Parse(Path.Combine(directory.FullName, "flow.cmg"), """
            import "shared.cmg"
            test "uses import" {
              call helper "#run"
            }
            """);

            Assert.True(result.Success, result.Error);
            Assert.Equal("macro", result.Document!.Nodes[0].Kind);
            Assert.Equal("test", result.Document.Nodes[1].Kind);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}

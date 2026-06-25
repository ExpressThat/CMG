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
          type "#name" "Line\nTwo" timeout=5000
        }
        """);

        var action = Assert.Single(Assert.Single(result.Document!.Nodes).Children);
        Assert.Equal("Line\nTwo", action.Arguments[1]);
        Assert.Equal("5000", action.Options["timeout"]);
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
}

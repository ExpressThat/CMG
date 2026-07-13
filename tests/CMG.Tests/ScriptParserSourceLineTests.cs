using CMG.Browser.Scripting;
using CMG.Runner;

namespace CMG.Tests;

public sealed class ScriptParserSourceLineTests
{
    [Fact]
    public void DirectParser_PreservesPhysicalLinesAcrossCommentsAndDenseActions()
    {
        var script = "# comment\n\ncaption \"ready\"\nif true { click \"#one\"; click \"#two\" }";

        var result = new BrowserScriptParser().Parse(script);

        Assert.True(result.Success, result.Error);
        Assert.Equal(3, result.Actions[0].LineNumber);
        Assert.Equal(4, result.Actions[1].LineNumber);
        Assert.All(result.Actions[1].Children, action => Assert.Equal(4, action.LineNumber));
    }

    [Fact]
    public void RunnerParser_PreservesPhysicalLinesForNestedChildren()
    {
        var script = "# comment\n\ntest \"flow\" {\n  narrate \"save\" {\n    click \"#save\"; evaluate \"true\"\n  }\n}";

        var result = new CmgDslParser().Parse("flow.cmgscript", script);

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(result.Document!.Nodes);
        var narrate = Assert.Single(test.Children);
        Assert.Equal(3, test.LineNumber);
        Assert.Equal(4, narrate.LineNumber);
        Assert.All(narrate.Children, child => Assert.Equal(5, child.LineNumber));
    }
}

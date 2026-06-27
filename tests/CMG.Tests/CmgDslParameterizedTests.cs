using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgDslParameterizedTests
{
    [Fact]
    public void Parse_ValidatesParameterizedTests()
    {
        var missingValues = new CmgDslParser().Parse("flow.cmgscript", """
        test.each "case" {
          caption "never"
        }
        """);

        Assert.False(missingValues.Success);
        Assert.Contains("requires values=, each=, or json=", missingValues.Error);

        var badJson = new CmgDslParser().Parse("flow.cmgscript", """
        test "case" json="{\"name\":\"Profile\"}" {
          caption "never"
        }
        """);

        Assert.False(badJson.Success);
        Assert.Contains("json= must be a JSON array", badJson.Error);
    }
}

using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptParserTests
{
    [Fact]
    public void Parse_AllowsDottedOptionKeys()
    {
        var result = new BrowserScriptParser().Parse("apiRequest \"GET\" \"https://example.test\" query.preview=true header.Authorization=\"Bearer token\"");
        var action = Assert.Single(result.Actions);

        Assert.True(result.Success);
        Assert.Equal("true", action.Options["query.preview"]);
        Assert.Equal("Bearer token", action.Options["header.Authorization"]);
        Assert.Equal(2, action.Arguments.Count);
    }
}

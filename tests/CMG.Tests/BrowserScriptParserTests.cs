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

    [Fact]
    public void Parse_PreservesEmptyQuotedStrings()
    {
        var result = new BrowserScriptParser().Parse("if \"\" == \"\" {\n  evaluate \"true\"\n}");

        Assert.True(result.Success);
        var action = Assert.Single(result.Actions);
        Assert.Equal("", action.Arguments[0]);
        Assert.Equal("", action.Arguments[2]);
    }

    [Fact]
    public void Parse_AllowsOddBranchSpacing()
    {
        var result = new BrowserScriptParser().Parse("""
        if false {
          caption "if"
        }   ELSE   {
          caption "else"
        }
        try {
          caption "try"
        }catch error{
          caption "${error}"
        }   FINALLY   {
          caption "done"
        }
        """);

        Assert.True(result.Success, result.Error);
        Assert.Equal(["if", "else", "try", "catch", "finally"], result.Actions.Select(action => action.Name.ToLowerInvariant()));
    }
}

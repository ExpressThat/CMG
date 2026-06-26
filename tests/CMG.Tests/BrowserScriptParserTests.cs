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
    public void Parse_TreatsRichLocatorTokenAsOption()
    {
        var result = new BrowserScriptParser().Parse("waitForSelector text=Ready");
        var action = Assert.Single(result.Actions);

        Assert.True(result.Success);
        Assert.Empty(action.Arguments);
        Assert.Equal("Ready", action.Options["text"]);
    }

    [Fact]
    public void Parse_TreatsProviderLocatorTokenAsOption()
    {
        var result = new BrowserScriptParser().Parse("click getByRole=button|Save");
        var action = Assert.Single(result.Actions);

        Assert.True(result.Success);
        Assert.Empty(action.Arguments);
        Assert.Equal("button|Save", action.Options["getByRole"]);
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

    [Fact]
    public void Parse_AllowsInlineBlocksWithoutSplittingQuotedBraces()
    {
        var result = new BrowserScriptParser().Parse("""
        if true { setContent "<main>{ok}</main>" } else { evaluate "({ value: 'no' })" }
        """);

        Assert.True(result.Success, result.Error);
        var action = result.Actions[0];
        Assert.Equal("if", action.Name);
        Assert.Equal("<main>{ok}</main>", Assert.Single(action.Children).Arguments[0]);
        Assert.Equal("else", result.Actions[1].Name);
    }

    [Fact]
    public void Parse_AllowsHeavyIndentationAndRepeatedSpacing()
    {
        var result = new BrowserScriptParser().Parse("          click          \"#save\"          timeout=5000\r\n\t\tcaption          \"Done\"");

        Assert.True(result.Success, result.Error);
        Assert.Collection(
            result.Actions,
            action =>
            {
                Assert.Equal("click", action.Name);
                Assert.Equal("#save", action.Arguments[0]);
                Assert.Equal("5000", action.Options["timeout"]);
            },
            action =>
            {
                Assert.Equal("caption", action.Name);
                Assert.Equal("Done", action.Arguments[0]);
            });
    }

    [Fact]
    public void Parse_AllowsMessySpacingInsideNestedBlocks()
    {
        var result = new BrowserScriptParser().Parse("""
                    if          true          {
                              click          "#save"          timeout=5000
                    }          else          {
                              caption          "not ready"
                    }
        """);

        Assert.True(result.Success, result.Error);
        var action = result.Actions[0];
        Assert.Equal("if", action.Name);
        Assert.Equal("true", action.Arguments[0]);
        Assert.Equal("#save", Assert.Single(action.Children).Arguments[0]);
        Assert.Equal("else", result.Actions[1].Name);
    }

    [Fact]
    public void Parse_AllowsSemicolonSeparatedActionsOutsideQuotes()
    {
        var result = new BrowserScriptParser().Parse("""
        caption "one;still one"; if true { caption "two"; caption "three" }; caption "four"
        """);

        Assert.True(result.Success, result.Error);
        Assert.Equal(["caption", "if", "caption"], result.Actions.Select(action => action.Name.ToLowerInvariant()));
        Assert.Equal("one;still one", result.Actions[0].Arguments[0]);
        Assert.Equal(["caption", "caption"], result.Actions[1].Children.Select(action => action.Name.ToLowerInvariant()));
    }

    [Fact]
    public void Parse_IgnoresInlineCommentsOutsideQuotesAndSelectors()
    {
        var result = new BrowserScriptParser().Parse("""
        # full-line comment
        click #save # css id token stays intact
        caption "literal # comment stays text"; assertText "#status" "Saved #1" # trailing note
        if true { caption "# inside dense block" # comment before close
        }
        """);

        Assert.True(result.Success, result.Error);
        Assert.Equal(["click", "caption", "assertText", "if"], result.Actions.Select(action => action.Name));
        Assert.Equal("#save", result.Actions[0].Arguments[0]);
        Assert.Equal("literal # comment stays text", result.Actions[1].Arguments[0]);
        Assert.Equal("Saved #1", result.Actions[2].Arguments[1]);
        Assert.Equal("# inside dense block", Assert.Single(result.Actions[3].Children).Arguments[0]);
    }
}

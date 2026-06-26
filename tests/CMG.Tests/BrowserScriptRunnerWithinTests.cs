using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerWithinTests
{
    [Fact]
    public void RunText_WithinScopesPointerAndFormActions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        within ".card" {
          click ".save"
          type "input[name=email]" "agent@example.com"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal(".card .save", client.LastClickedSelector);
        Assert.Equal(".card input[name=email]", client.LastTypedSelector);
    }

    [Fact]
    public void RunText_WithinScopesNestedBlocks()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        within ".shell" {
          within ".dialog" {
            hover ".close"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal(".shell .dialog .close", client.LastHoveredSelector);
    }

    [Fact]
    public void RunText_WithinScopesSingleArgumentTextAssertions()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved successfully");
        var result = Runner().RunText("""
        within ".toast" {
          contains "Saved"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal(".toast", client.LastElementTextSelector);
    }

    [Fact]
    public void RunText_WithinScopesSelectorIteration()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("2");
        var result = Runner().RunText("""
        within ".list" {
          foreachSelector row ".item" {
            click "${row}"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("document.querySelectorAll('.list .item').length", client.EvaluatedExpressions[0]);
        Assert.Equal(".list #__cmg_foreach_2_1", client.LastClickedSelector);
    }

    [Fact]
    public void RunText_WithinToleratesWeirdSpacingAndInlineBlocks()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""	  within      ".toolbar"      {   click      ".save"   ; within   ".menu" { click ".item" }   }""", "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal(".toolbar .menu .item", client.LastClickedSelector);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

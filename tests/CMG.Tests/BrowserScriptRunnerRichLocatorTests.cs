using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerRichLocatorTests
{
    [Fact]
    public void RunText_ClickResolvesTextLocator()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("click text=Save", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("No element matched locator text=Save", client.EvaluatedExpressions[0]);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastClickedSelector);
    }

    [Fact]
    public void RunText_TypePrependsLocatorOptionBeforeText()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("type label=Email \"agent@example.com\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastTypedSelector);
        Assert.Equal("agent@example.com", client.LastTypedText);
    }

    [Fact]
    public void RunText_AssertTextUsesResolvedLocator()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved");
        var result = Runner().RunText("assertText role=status Saved", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastElementTextSelector);
    }

    [Fact]
    public void RunText_ClickResolvesFilterLocatorOption()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("click nth=.item|1", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("querySelectorAll('.item')", client.EvaluatedExpressions[0]);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastClickedSelector);
    }

    [Theory]
    [InlineData("has=.item|.badge", "e.querySelector('.badge')")]
    [InlineData("hasNot=.item|.badge", "!e.querySelector('.badge')")]
    [InlineData("hasNotText=.item|Draft", "includes('Draft')")]
    [InlineData("textExact=Save", ".trim() === 'Save'")]
    [InlineData("textRegex=^Save", "new RegExp('^Save')")]
    [InlineData("role=button|Save", "accessibleName(e).includes('Save')")]
    [InlineData("roleRegex=button|^Save", "new RegExp('^Save').test(accessibleName(e))")]
    [InlineData("labelExact=Email", ".trim() === 'Email'")]
    [InlineData("labelRegex=^Email", "new RegExp('^Email')")]
    [InlineData("or=.primary|.secondary", "document.querySelector('.primary') ?? document.querySelector('.secondary')")]
    [InlineData("and=.item|.selected", "e.matches('.selected')")]
    [InlineData("shadow=#host|button.save", "shadowRoot?.querySelector('button.save')")]
    [InlineData("shadowText=#host|Shadow Save", "shadowRoot?.querySelectorAll('*')")]
    public void RunText_ClickResolvesAdvancedFilterLocatorOptions(string locator, string expectedExpression)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"click \"{locator}\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(expectedExpression, client.EvaluatedExpressions[0]);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastClickedSelector);
    }

    [Fact]
    public void RunText_MouseMoveSelectorUsesResolvedLocator()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("mouseMove selector=\"text=Save\" edge=center", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastElementBoxSelector);
    }

    [Fact]
    public void RunText_ExpectationAcceptsGenericExactLocatorOptions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectVisible shadow=#host|button.save", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("shadowRoot?.querySelector('button.save')", client.EvaluatedExpressions[0]);
        Assert.Contains("__cmgQuery", client.EvaluatedExpressions[1]);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

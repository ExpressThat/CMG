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

    [Fact]
    public void RunText_MouseMoveSelectorUsesResolvedLocator()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("mouseMove selector=\"text=Save\" edge=center", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastElementBoxSelector);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

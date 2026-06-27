using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerSelectOptionTests
{
    [Theory]
    [InlineData("selectOption #plan optionLabel=Pro", "option.label === \"Pro\"", "label 'Pro'")]
    [InlineData("selectOption #plan optionValue=pro", "option.value === \"pro\"", "value 'pro'")]
    [InlineData("selectOption #plan index=2", "options[2]", "index 2")]
    public void RunText_SelectOptionSupportsProviderTargets(string script, string expectedScript, string expectedFailureText)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(expectedScript, client.LastExpression);
        Assert.Contains(expectedFailureText, client.LastExpression);
    }

    [Fact]
    public void RunText_SelectOptionTargetSupportsRichLocator()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("selectOption label=Plan optionValue=pro", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastExpression);
        Assert.Contains("option.value === \"pro\"", client.LastExpression);
    }

    [Fact]
    public void RunText_SelectOptionIndexValidatesInput()
    {
        var result = Runner().RunText("selectOption #plan index=-1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("index= must be zero or greater", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

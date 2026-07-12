using CMG.Browser;

namespace CMG.Tests;

public sealed class ChromeDevToolsClientInputScriptTests
{
    [Fact]
    public void BuildInputValueScript_UsesNativeSetterAndInputEvent()
    {
        var script = ChromeDevToolsClient.BuildInputValueScript("'Ross'", "Ross");

        Assert.Contains("Object.getOwnPropertyDescriptor(prototype, 'value')?.set", script);
        Assert.Contains("setter.call(element, 'Ross')", script);
        Assert.Contains("new InputEvent('input'", script);
        Assert.Contains("inputType: 'insertText'", script);
        Assert.Contains("data: \"Ross\"", script);
    }

    [Fact]
    public void BuildInputValueScript_EscapesInputEventData()
    {
        var script = ChromeDevToolsClient.BuildInputValueScript("''", "a\"b");

        Assert.Contains("data: \"a\\u0022b\"", script);
    }
}

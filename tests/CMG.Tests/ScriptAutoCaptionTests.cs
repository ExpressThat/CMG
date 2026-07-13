using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptAutoCaptionTests
{
    [Fact]
    public void Fill_DefaultCaptionDoesNotExposeEnteredValue()
    {
        var action = Action("fill", ["#token", "secret-value"]);

        Assert.True(ScriptAutoCaption.TryCreate(action, out var caption));
        Assert.Equal("Enter text in #token", caption.Message);
        Assert.DoesNotContain("secret-value", caption.Message);
    }

    [Fact]
    public void Template_ReplacesSupportedPlaceholders()
    {
        var action = Action("dragAndDrop", ["#card", "#done"], "{action}: {selector} -> {target} at {line}");

        Assert.True(ScriptAutoCaption.TryCreate(action, out var caption));
        Assert.Equal("dragAndDrop: #card -> #done at 7", caption.Message);
    }

    [Fact]
    public void Template_RejectsUnknownPlaceholder()
    {
        var action = Action("click", ["#save"], "Click {unknown}");

        var error = Assert.Throws<ScriptExecutionException>(() => ScriptAutoCaption.TryCreate(action, out _));
        Assert.Contains("unknown placeholder '{unknown}'", error.Message);
    }

    [Fact]
    public void DisabledAction_DoesNotCreateCaption()
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["autoCaptions"] = "false" };
        var action = new BrowserScriptAction(7, "click", "click", ["#save"], options, []);

        Assert.False(ScriptAutoCaption.TryCreate(action, out _));
    }

    private static BrowserScriptAction Action(string name, IReadOnlyList<string> arguments, string? template = null)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["autoCaptions"] = "true" };
        if (template is not null)
        {
            options["captionTemplate"] = template;
        }
        return new BrowserScriptAction(7, name, name, arguments, options, []);
    }
}

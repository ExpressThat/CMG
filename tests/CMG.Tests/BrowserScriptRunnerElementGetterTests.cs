using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerElementGetterTests
{
    [Theory]
    [InlineData("textContent", "TEXT")]
    [InlineData("innerText", "TEXT")]
    public void RunText_TextGettersReturnElementText(string action, string label)
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved");

        var result = Runner().RunText($"{action} \"#status\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#status", client.LastElementTextSelector);
        Assert.Contains($"{label} 001 Saved", result.StdoutLines);
    }

    [Fact]
    public void RunText_InputValueReturnsValuePayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("agent@example.com");

        var result = Runner().RunText("inputValue \"#email\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("element.value", client.LastExpression);
        Assert.Contains("VALUE 001 agent@example.com", result.StdoutLines);
    }

    [Fact]
    public void RunText_CountReturnsCountPayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("3");

        var result = Runner().RunText("count \".row\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgQueryAll?.(\".row\")", client.LastExpression);
        Assert.Contains("COUNT 001 3", result.StdoutLines);
    }

    [Fact]
    public void RunText_BoundingBoxReturnsJsonPayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"x":1,"y":2,"width":3,"height":4}""");

        var result = Runner().RunText("boundingBox \"#card\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("getBoundingClientRect", client.LastExpression);
        Assert.Contains("""BOUNDING_BOX 001 {"x":1,"y":2,"width":3,"height":4}""", result.StdoutLines);
    }

    [Theory]
    [InlineData("allTextContents", "textContent")]
    [InlineData("allInnerTexts", "innerText")]
    public void RunText_AllTextGettersReturnJsonPayload(string action, string property)
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""["One","Two"]""");

        var result = Runner().RunText($"{action} \".item\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains($".{property}", client.LastExpression);
        Assert.Contains("""TEXTS 001 ["One","Two"]""", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetBlockStoresAttributePayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("/profile");
        client.EvaluateResponses.Enqueue("/profile");

        var result = Runner().RunText("""
        set href {
          getAttribute "#profile" "href"
        }
        evaluate "'${href}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("ATTRIBUTE 002 /profile", result.StdoutLines);
        Assert.Contains("SET 001 href /profile", result.StdoutLines);
    }

    [Theory]
    [InlineData("computedStyle \"#status\" \"display\"", "getComputedStyle(element)", "STYLE 001 block", "block")]
    [InlineData("property \"#status\" \"dataset.state\"", "const path = \"dataset.state\".split('.')", "PROPERTY 001 ready", "ready")]
    public void RunText_InspectionGettersReturnPayload(
        string script,
        string expectedExpression,
        string expectedOutput,
        string payload)
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue(payload);

        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(expectedExpression, client.LastExpression);
        Assert.Contains(expectedOutput, result.StdoutLines);
    }

    [Fact]
    public void RunText_SetBlockStoresComputedStylePayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("grid");
        client.EvaluateResponses.Enqueue("grid");

        var result = Runner().RunText("""
        set display {
          computedStyle "#app" "display"
        }
        evaluate "'${display}'"
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("SET 001 display grid", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetBlockStoresCountPayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("2");
        client.EvaluateResponses.Enqueue("2");

        var result = Runner().RunText("""
        set total {
          count ".item"
        }
        evaluate "'${total}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("SET 001 total 2", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

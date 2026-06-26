using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerCollectionLoopTests
{
    [Fact]
    public void RunText_ForEachJsonIteratesArrayValuesAndIndex()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        set names "[\"Ada\",\"Grace\"]"
        foreachJson name "${names}" {
          type "#name-${index}" "${name}"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#name-1", client.LastTypedSelector);
        Assert.Equal("Grace", client.LastTypedText);
    }

    [Fact]
    public void RunText_ForEachJsonKeepsObjectsAsJson()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        foreachJson item "[{\"name\":\"Ada\"}]" {
          type "#json" "${item}"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("{\"name\":\"Ada\"}", client.LastTypedText);
    }

    [Fact]
    public void RunText_ForEachListSplitsAndTrimsValues()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        foreachList item "alpha | beta | gamma" delimiter="|" {
          type "#item-${index}" "${item}"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#item-2", client.LastTypedSelector);
        Assert.Equal("gamma", client.LastTypedText);
    }

    [Fact]
    public void RunText_ForEachJsonFailsClearlyForNonArray()
    {
        var result = Runner().RunText("foreachJson item \"{}\" { caption nope }", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("foreachJson expects a JSON array", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}

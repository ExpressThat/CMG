using System.Reflection;
using System.Text.Json;
using CMG.Browser;

namespace CMG.Tests;

public sealed class ChromeDevToolsClientWorkerResultTests
{
    [Theory]
    [InlineData("""{"result":{"result":{"type":"number","value":2}}}""", "2")]
    [InlineData("""{"result":{"result":{"type":"boolean","value":true}}}""", "True")]
    [InlineData("""{"result":{"result":{"type":"string","value":"ready"}}}""", "ready")]
    public void ReadScriptResult_ReturnsPrimitiveValues(string json, string expected)
    {
        using var document = JsonDocument.Parse(json);

        var actual = ReadScriptResult(document.RootElement);

        Assert.Equal(expected, actual);
    }

    private static string ReadScriptResult(JsonElement element)
    {
        var method = typeof(ChromeDevToolsClient).GetMethod(
            "ReadScriptResult",
            BindingFlags.NonPublic | BindingFlags.Static);
        return (string)(method?.Invoke(null, [element]) ?? string.Empty);
    }
}

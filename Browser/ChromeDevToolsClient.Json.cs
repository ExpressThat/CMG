using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private static string BuildOuterHtmlExpression(string selector)
    {
        return $"(() => {{ const element = {BrowserDomScripts.Query(selector)}; return element ? element.outerHTML : null; }})()";
    }

    private static string BuildElementActionExpression(string selector, string body) =>
        BrowserDomScripts.ElementAction(selector, body);

    private static int ReadInt32(JsonElement root, params string[] path)
    {
        if (!TryReadElement(root, path, out var element) || !element.TryGetInt32(out var value))
        {
            throw new ChromeDevToolsException($"Chrome response did not contain expected field '{string.Join('.', path)}'.");
        }

        return value;
    }

    private static bool TryReadString(JsonElement root, string propertyName, out string? value)
    {
        value = null;

        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static bool TryReadString(JsonElement root, IReadOnlyList<string> path, out string? value)
    {
        value = null;

        if (!TryReadElement(root, path, out var element) || element.ValueKind is JsonValueKind.Null)
        {
            return false;
        }

        if (element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static bool TryReadBoolean(JsonElement root, IReadOnlyList<string> path, out bool value)
    {
        value = false;

        if (!TryReadElement(root, path, out var element) || element.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
        {
            return false;
        }

        value = element.GetBoolean();
        return true;
    }

    private static bool TryReadDouble(JsonElement root, string propertyName, out double value)
    {
        value = 0;

        return root.TryGetProperty(propertyName, out var element) && element.TryGetDouble(out value);
    }

    private static string ToJsonStringLiteral(string value)
    {
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStringValue(value);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static bool TryReadString(JsonElement root, string first, string second, out string? value)
    {
        value = null;

        if (!TryReadElement(root, [first, second], out var element) || element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static bool TryReadElement(JsonElement root, IReadOnlyList<string> path, out JsonElement element)
    {
        element = root;

        foreach (var propertyName in path)
        {
            if (!element.TryGetProperty(propertyName, out element))
            {
                return false;
            }
        }

        return true;
    }
}

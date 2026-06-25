using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private void ExecuteElementScript(string remoteDebuggingUrl, string selector, string body)
    {
        if (Evaluate(remoteDebuggingUrl, BrowserDomScripts.ElementAction(selector, body)) is not "true")
        {
            throw new ElementNotFoundException(selector);
        }
    }

    private void ExecuteVisibleElementScript(string remoteDebuggingUrl, string selector, string body)
    {
        _ = GetElementCenter(remoteDebuggingUrl, selector);
        ExecuteElementScript(remoteDebuggingUrl, selector, body);
    }

    private static async Task<JsonElement> Evaluate(FirefoxBiDiSession session, string contextId, string expression) =>
        await session.SendCommand("script.evaluate", writer =>
        {
            writer.WriteString("expression", expression);
            writer.WriteBoolean("awaitPromise", true);
            writer.WriteString("resultOwnership", "none");
            writer.WriteStartObject("target");
            writer.WriteString("context", contextId);
            writer.WriteEndObject();
        });

    private static async Task PromoteMessageBar(FirefoxBiDiSession session, string contextId) =>
        await Evaluate(session, contextId, BrowserDomScripts.PromoteMessageBar());

    private static async Task<ElementRect> GetElementRect(FirefoxBiDiSession session, string contextId, string selector)
    {
        var json = ReadScriptResultValue(await Evaluate(session, contextId, BrowserDomScripts.ElementRect(selector)));
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ElementNotFoundException(selector);
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return new ElementRect(
            root.GetProperty("x").GetDouble(),
            root.GetProperty("y").GetDouble(),
            root.GetProperty("width").GetDouble(),
            root.GetProperty("height").GetDouble());
    }

    private static async Task EnsurePointInViewport(FirefoxBiDiSession session, string contextId, string selector, double x, double y)
    {
        var json = ReadScriptResultValue(await Evaluate(
            session,
            contextId,
            "JSON.stringify({ width: window.innerWidth, height: window.innerHeight })"));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var width = root.GetProperty("width").GetDouble();
        var height = root.GetProperty("height").GetDouble();

        if (x < 0 || y < 0 || x > width || y > height)
        {
            throw new ChromeDevToolsException($"Element '{selector}' is outside the current viewport. Run scrollIntoView first if this movement should scroll the page.");
        }
    }

    private static string NonEmpty(string value, string selector) =>
        string.IsNullOrEmpty(value) ? throw new ElementNotFoundException(selector) : value;

    private static byte[] DecodeScreenshot(JsonElement response)
    {
        if (!TryReadString(response, ["result", "data"], out var data) || string.IsNullOrWhiteSpace(data))
        {
            throw new ChromeDevToolsException("Firefox did not return screenshot image data.");
        }

        return Convert.FromBase64String(data);
    }

    private static string ReadScriptResultValue(JsonElement response)
    {
        if (!TryReadElement(response, ["result", "result"], out var result) ||
            !TryReadString(result, "type", out var type) ||
            type is "undefined" or "null")
        {
            return string.Empty;
        }

        if (!result.TryGetProperty("value", out var value))
        {
            return TryReadString(result, "text", out var text) ? text ?? string.Empty : string.Empty;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.ToString(),
            _ => value.ToString()
        };
    }

    private static T Run<T>(Func<Task<T>> action)
    {
        try
        {
            return action().GetAwaiter().GetResult();
        }
        catch (AggregateException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }

    private static bool TryReadString(JsonElement root, IReadOnlyList<string> path, out string? value)
    {
        value = null;
        if (!TryReadElement(root, path, out var element) || element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
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

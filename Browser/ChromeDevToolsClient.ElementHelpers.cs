using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private static T Run<T>(Func<Task<T>> action)
    {
        return action().GetAwaiter().GetResult();
    }

    private void ExecuteElementScript(string remoteDebuggingUrl, string selector, string body)
    {
        _ = EvaluateElementScript(remoteDebuggingUrl, selector, body);
    }

    private string EvaluateElementScript(string remoteDebuggingUrl, string selector, string body)
    {
        return Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);

            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", BuildElementActionExpression(selector, body));
                writer.WriteBoolean("returnByValue", true);
            });

            if (!TryReadElement(response, ["result", "result"], out var result))
            {
                return string.Empty;
            }

            if (result.TryGetProperty("value", out var value))
            {
                return value.ValueKind is JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
            }

            return result.TryGetProperty("description", out var description) ? description.GetString() ?? string.Empty : string.Empty;
        });
    }

    private static async Task<ElementClip> GetElementClip(DevToolsSession session, string selector)
    {
        var response = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                $"JSON.parse({BrowserDomScripts.ElementRect(selector)})");
            writer.WriteBoolean("returnByValue", true);
        });

        if (!TryReadElement(response, ["result", "result", "value"], out var value) || value.ValueKind is JsonValueKind.Null)
        {
            throw new ElementNotFoundException(selector);
        }

        if (!TryReadDouble(value, "x", out var x) ||
            !TryReadDouble(value, "y", out var y) ||
            !TryReadDouble(value, "width", out var width) ||
            !TryReadDouble(value, "height", out var height))
        {
            throw new ChromeDevToolsException($"Chrome did not return a viewport rectangle for element '{selector}'.");
        }

        var interactionX = TryReadDouble(value, "interactionX", out var ix) ? ix : (double?)null;
        var interactionY = TryReadDouble(value, "interactionY", out var iy) ? iy : (double?)null;
        var clip = new ElementClip(x, y, width, height, interactionX, interactionY);
        if (clip.Width <= 0 || clip.Height <= 0)
        {
            throw new ChromeDevToolsException($"Element '{selector}' has no visible area.");
        }

        return clip;
    }

    private static async Task PromoteMessageBar(DevToolsSession session)
    {
        await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                BrowserDomScripts.PromoteMessageBar());
            writer.WriteBoolean("returnByValue", true);
        });
    }

    private static async Task ClickAt(DevToolsSession session, double x, double y)
    {
        await session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mousePressed");
            writer.WriteNumber("x", x);
            writer.WriteNumber("y", y);
            writer.WriteString("button", "left");
            writer.WriteNumber("clickCount", 1);
        });

        await session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mouseReleased");
            writer.WriteNumber("x", x);
            writer.WriteNumber("y", y);
            writer.WriteString("button", "left");
            writer.WriteNumber("clickCount", 1);
        });
    }

    private static async Task EnsurePointInViewport(DevToolsSession session, string selector, double x, double y)
    {
        var response = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                $"(() => {{" +
                $"const x = {x.ToString(System.Globalization.CultureInfo.InvariantCulture)};" +
                $"const y = {y.ToString(System.Globalization.CultureInfo.InvariantCulture)};" +
                "return x >= 0 && y >= 0 && x <= window.innerWidth && y <= window.innerHeight;" +
                "})()");
            writer.WriteBoolean("returnByValue", true);
        });

        if (!TryReadBoolean(response, ["result", "result", "value"], out var inViewport) || !inViewport)
        {
            throw new ChromeDevToolsException($"Element '{selector}' is outside the current viewport. Run scrollIntoView first if this movement should scroll the page.");
        }
    }

    private static Task DispatchMouseMove(DevToolsSession session, ElementPoint point, int buttons)
    {
        return session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mouseMoved");
            writer.WriteNumber("x", point.X);
            writer.WriteNumber("y", point.Y);
            writer.WriteString("button", buttons is 0 ? "none" : "left");
            writer.WriteNumber("buttons", buttons);
        });
    }

    private static Task DispatchMousePressed(DevToolsSession session, ElementPoint point)
    {
        return session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mousePressed");
            writer.WriteNumber("x", point.X);
            writer.WriteNumber("y", point.Y);
            writer.WriteString("button", "left");
            writer.WriteNumber("buttons", 1);
            writer.WriteNumber("clickCount", 1);
        });
    }

    private static Task DispatchMouseReleased(DevToolsSession session, ElementPoint point)
    {
        return session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mouseReleased");
            writer.WriteNumber("x", point.X);
            writer.WriteNumber("y", point.Y);
            writer.WriteString("button", "left");
            writer.WriteNumber("buttons", 0);
            writer.WriteNumber("clickCount", 1);
        });
    }
}

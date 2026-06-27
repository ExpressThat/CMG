using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public void ShowMessageBar(string remoteDebuggingUrl, string message)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowMessageBar(message));
    }

    public void PromoteMessageBar(string remoteDebuggingUrl)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.PromoteMessageBar());
    }

    public string GetElementText(string remoteDebuggingUrl, string selector)
    {
        return EvaluateElementScript(remoteDebuggingUrl, selector, "return element.innerText ?? element.textContent ?? '';");
    }

    public string Evaluate(string remoteDebuggingUrl, string expression)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", expression);
                writer.WriteBoolean("returnByValue", true);
                writer.WriteBoolean("awaitPromise", true);
            });
            if (response.TryGetProperty("exceptionDetails", out var exception))
            {
                throw new ChromeDevToolsException(ReadEvaluationException(exception));
            }

            if (!TryReadElement(response, ["result", "result"], out var result))
            {
                return string.Empty;
            }

            if (result.TryGetProperty("subtype", out var subtype) &&
                subtype.GetString() is "error" &&
                result.TryGetProperty("description", out var error))
            {
                throw new ChromeDevToolsException(error.GetString() ?? "JavaScript evaluation failed.");
            }

            if (result.TryGetProperty("value", out var value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString() ?? string.Empty,
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => value.ToString()
                };
            }

            return result.TryGetProperty("description", out var description) ? description.GetString() ?? string.Empty : string.Empty;
        });
    }

    private static string ReadEvaluationException(JsonElement exception)
    {
        if (exception.TryGetProperty("exception", out var error) &&
            error.TryGetProperty("description", out var description))
        {
            return description.GetString() ?? "JavaScript evaluation failed.";
        }

        return exception.TryGetProperty("text", out var text)
            ? text.GetString() ?? "JavaScript evaluation failed."
            : "JavaScript evaluation failed.";
    }

    public void SetViewport(string remoteDebuggingUrl, ViewportOptions options)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await session.SendCommand("Emulation.setDeviceMetricsOverride", writer =>
            {
                writer.WriteNumber("width", options.Width);
                writer.WriteNumber("height", options.Height);
                writer.WriteNumber("deviceScaleFactor", options.DeviceScaleFactor);
                writer.WriteBoolean("mobile", options.IsMobile);
            });
            await session.SendCommand("Emulation.setTouchEmulationEnabled", writer =>
            {
                writer.WriteBoolean("enabled", options.HasTouch);
                if (options.HasTouch)
                {
                    writer.WriteNumber("maxTouchPoints", 1);
                }
            });
            if (options.HasTouch || options.IsMobile)
            {
                await session.SendCommand("Runtime.evaluate", writer =>
                {
                    writer.WriteString("expression", "Object.defineProperty(navigator, 'maxTouchPoints', { configurable: true, get: () => 1 }); window.ontouchstart = window.ontouchstart || null; true");
                    writer.WriteBoolean("returnByValue", true);
                });
            }

            return true;
        });
    }

    public ViewportSize GetViewportSize(string remoteDebuggingUrl)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", "({ width: window.innerWidth, height: window.innerHeight })");
                writer.WriteBoolean("returnByValue", true);
            });

            if (!TryReadElement(response, ["result", "result", "value"], out var value) ||
                !TryReadDouble(value, "width", out var width) ||
                !TryReadDouble(value, "height", out var height))
            {
                throw new ChromeDevToolsException("Chrome did not return the current viewport size.");
            }

            return new ViewportSize(width, height);
        });
    }

    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector)
    {
        Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, sourceSelector) ??
                throw new ElementNotFoundException(sourceSelector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            var source = await GetElementClip(session, sourceSelector);
            var target = await GetElementClip(session, targetSelector);
            await EnsurePointInViewport(session, sourceSelector, source.CenterX, source.CenterY);
            await EnsurePointInViewport(session, targetSelector, target.CenterX, target.CenterY);

            var expression = BrowserDomScripts.DragAndDrop(sourceSelector, targetSelector);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", expression);
                writer.WriteBoolean("returnByValue", true);
            });

            if (!TryReadBoolean(response, ["result", "result", "value"], out var success) || !success)
            {
                throw new ElementNotFoundException(targetSelector);
            }

            return true;
        });
    }

    public void MouseDragAndDrop(
        string remoteDebuggingUrl,
        string sourceSelector,
        string targetSelector,
        IReadOnlyList<ElementPoint> path,
        Action<ElementPoint>? afterMove = null)
    {
        Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, sourceSelector) ??
                throw new ElementNotFoundException(sourceSelector);

            await using var session = await DevToolsSession.Connect(pageTarget);

            var source = await GetElementClip(session, sourceSelector);
            var target = await GetElementClip(session, targetSelector);
            await EnsurePointInViewport(session, sourceSelector, source.CenterX, source.CenterY);
            await EnsurePointInViewport(session, targetSelector, target.CenterX, target.CenterY);
            var start = new ElementPoint(source.CenterX, source.CenterY);
            var points = path.Count > 0 ? path : [start];

            await DispatchMouseMove(session, start, buttons: 0);
            await DispatchMousePressed(session, start);
            afterMove?.Invoke(start);

            foreach (var point in points)
            {
                await DispatchMouseMove(session, point, buttons: 1);
                afterMove?.Invoke(point);
            }

            await DispatchMouseReleased(session, points[^1]);

            return true;
        });
    }

    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.BeginDrag(sourceSelector, point));
    }

    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDrag(point));
    }

    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.EndDrag(point));
    }

    public void RemoveDefaultDragGhost(string remoteDebuggingUrl)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveDefaultDragGhost());
    }

    public void MoveMouse(string remoteDebuggingUrl, ElementPoint point, int buttons)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await DispatchMouseMove(session, point, buttons);
            return true;
        });
    }

    public void MouseDown(string remoteDebuggingUrl, ElementPoint point)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await DispatchMousePressed(session, point);
            return true;
        });
    }

    public void MouseUp(string remoteDebuggingUrl, ElementPoint point)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await DispatchMouseReleased(session, point);
            return true;
        });
    }
}

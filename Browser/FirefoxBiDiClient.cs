using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed class FirefoxBiDiClient : IBrowserAutomationClient
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public string GetElementHtml(string remoteDebuggingUrl, string selector) =>
        NonEmpty(Evaluate(remoteDebuggingUrl, $"document.querySelector({BrowserDomScripts.JsonString(selector)})?.outerHTML ?? null"), selector);

    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            await Evaluate(session, context.Id, BrowserDomScripts.ScrollIntoView(selector));
            await PromoteMessageBar(session, context.Id);
            var rect = await GetElementRect(session, context.Id, selector);
            var response = await session.SendCommand("browsingContext.captureScreenshot", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteStartObject("clip");
                writer.WriteString("type", "box");
                writer.WriteNumber("x", rect.X);
                writer.WriteNumber("y", rect.Y);
                writer.WriteNumber("width", rect.Width);
                writer.WriteNumber("height", rect.Height);
                writer.WriteEndObject();
            });

            return DecodeScreenshot(response);
        });

    public string Navigate(string remoteDebuggingUrl, string target) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            var response = await session.SendCommand("browsingContext.navigate", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteString("url", target);
                writer.WriteString("wait", "complete");
            });

            if (TryReadString(response, ["result", "url"], out var url) &&
                !string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            throw new ChromeDevToolsException($"Firefox did not return a final URL after navigating to '{target}'.");
        });

    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds)
    {
        Run(async () =>
        {
            var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTimeOffset.UtcNow <= deadline)
            {
                if (Evaluate(remoteDebuggingUrl, $"Boolean(document.querySelector({BrowserDomScripts.JsonString(selector)}))") is "true")
                {
                    return true;
                }

                await Task.Delay(PollInterval);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public void Click(string remoteDebuggingUrl, string selector) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, "element.click(); return true;");

    public void Type(string remoteDebuggingUrl, string selector, string text) =>
        ExecuteVisibleElementScript(
            remoteDebuggingUrl,
            selector,
            $"element.focus({{ preventScroll: true }}); element.value = `${{element.value ?? ''}}{BrowserDomScripts.EscapeTemplate(text)}`; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");

    public void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null)
    {
        Click(remoteDebuggingUrl, selector);

        foreach (var character in text)
        {
            Type(remoteDebuggingUrl, selector, character.ToString());
            afterCharacter?.Invoke();
            Thread.Sleep(80);
        }
    }

    public void Clear(string remoteDebuggingUrl, string selector) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, "element.focus({ preventScroll: true }); element.value = ''; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;");

    public void Press(string remoteDebuggingUrl, string key) =>
        Evaluate(
            remoteDebuggingUrl,
            $"(() => {{ const target = document.activeElement || document.body; const options = {{ key: {BrowserDomScripts.JsonString(key)}, bubbles: true, cancelable: true }}; target.dispatchEvent(new KeyboardEvent('keydown', options)); target.dispatchEvent(new KeyboardEvent('keyup', options)); return true; }})()");

    public void Hover(string remoteDebuggingUrl, string selector) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, "const rect = element.getBoundingClientRect(); const options = { bubbles: true, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }; element.dispatchEvent(new MouseEvent('mouseover', options)); element.dispatchEvent(new MouseEvent('mousemove', options)); return true;");

    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector) =>
        ExecuteElementScript(remoteDebuggingUrl, selector, "element.scrollIntoView({ block: 'center', inline: 'center' }); return true;");

    public void Select(string remoteDebuggingUrl, string selector, string value) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, $"element.value = {BrowserDomScripts.JsonString(value)}; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");

    public void ShowMessageBar(string remoteDebuggingUrl, string message) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowMessageBar(message));

    public void PromoteMessageBar(string remoteDebuggingUrl) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.PromoteMessageBar());

    public string GetElementText(string remoteDebuggingUrl, string selector) =>
        NonEmpty(Evaluate(remoteDebuggingUrl, $"document.querySelector({BrowserDomScripts.JsonString(selector)})?.innerText ?? document.querySelector({BrowserDomScripts.JsonString(selector)})?.textContent ?? null"), selector);

    public string Evaluate(string remoteDebuggingUrl, string expression) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            return ReadScriptResultValue(await Evaluate(session, context.Id, expression));
        });

    public void SetViewport(string remoteDebuggingUrl, int width, int height) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            await session.SendCommand("browsingContext.setViewport", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteStartObject("viewport");
                writer.WriteNumber("width", width);
                writer.WriteNumber("height", height);
                writer.WriteEndObject();
                writer.WriteNumber("devicePixelRatio", 1);
            });

            return true;
        });

    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            var source = await GetElementRect(session, context.Id, sourceSelector);
            var target = await GetElementRect(session, context.Id, targetSelector);
            await EnsurePointInViewport(session, context.Id, sourceSelector, source.X + source.Width / 2, source.Y + source.Height / 2);
            await EnsurePointInViewport(session, context.Id, targetSelector, target.X + target.Width / 2, target.Y + target.Height / 2);
            _ = ReadScriptResultValue(await Evaluate(session, context.Id, BrowserDomScripts.DragAndDrop(sourceSelector, targetSelector)));
            return true;
        });

    public void MouseDragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector, IReadOnlyList<ElementPoint> path, Action<ElementPoint>? afterMove = null)
    {
        foreach (var point in path)
        {
            afterMove?.Invoke(point);
        }
    }

    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.BeginDrag(sourceSelector, point));

    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDrag(point));

    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.EndDrag(point));

    public void RemoveDefaultDragGhost(string remoteDebuggingUrl) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveDefaultDragGhost());

    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            if (promoteMessageBar)
            {
                await PromoteMessageBar(session, context.Id);
            }

            var response = await session.SendCommand("browsingContext.captureScreenshot", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteString("origin", "viewport");
            });

            return DecodeScreenshot(response);
        });

    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            var rect = await GetElementRect(session, context.Id, selector);
            await EnsurePointInViewport(session, context.Id, selector, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            return new ElementPoint(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        });

    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDomCursor(point));

    public void RemoveDomCursor(string remoteDebuggingUrl) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveDomCursor());

    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var contexts = await session.GetTopLevelContexts();
            var tabs = new List<ChromePageTab>();

            foreach (var context in contexts)
            {
                tabs.Add(new ChromePageTab(context.Id, ReadScriptResultValue(await Evaluate(session, context.Id, "document.title")), context.Url));
            }

            return tabs;
        });

    public void ActivateTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(index);
            await session.SendCommand("browsingContext.activate", writer => writer.WriteString("context", context.Id));
            return true;
        });

    public void CloseTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(index);
            await session.SendCommand("browsingContext.close", writer => writer.WriteString("context", context.Id));
            return true;
        });

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

    private sealed class FirefoxBiDiSession : IAsyncDisposable
    {
        private readonly ClientWebSocket socket;
        private int commandId;

        private FirefoxBiDiSession(ClientWebSocket socket)
        {
            this.socket = socket;
        }

        public static async Task<FirefoxBiDiSession> Connect(string remoteDebuggingUrl)
        {
            var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(NormalizeWebSocketUrl(remoteDebuggingUrl)), CancellationToken.None);
            var session = new FirefoxBiDiSession(socket);
            await session.SendCommand("session.new", writer =>
            {
                writer.WriteStartObject("capabilities");
                writer.WriteEndObject();
            });

            return session;
        }

        private static string NormalizeWebSocketUrl(string remoteDebuggingUrl)
        {
            var uri = new Uri(remoteDebuggingUrl);
            return string.IsNullOrWhiteSpace(uri.AbsolutePath) || uri.AbsolutePath is "/"
                ? $"{remoteDebuggingUrl.TrimEnd('/')}/session"
                : remoteDebuggingUrl;
        }

        public async Task<FirefoxContext> GetPrimaryContext() =>
            (await GetTopLevelContexts()).FirstOrDefault() ??
            throw new ChromeDevToolsException("No Firefox browsing context was available through WebDriver BiDi.");

        public async Task<FirefoxContext> GetContextAt(int index)
        {
            var contexts = await GetTopLevelContexts();
            if (index < 0 || index >= contexts.Count)
            {
                throw new ChromeDevToolsException($"Tab index {index} does not exist. Available tab count: {contexts.Count}.");
            }

            return contexts[index];
        }

        public async Task<IReadOnlyList<FirefoxContext>> GetTopLevelContexts()
        {
            var response = await SendCommand("browsingContext.getTree");
            if (!TryReadElement(response, ["result", "contexts"], out var contextsElement) ||
                contextsElement.ValueKind is not JsonValueKind.Array)
            {
                throw new ChromeDevToolsException("Firefox did not return browsing contexts.");
            }

            return contextsElement
                .EnumerateArray()
                .Where(context => TryReadString(context, "context", out _))
                .Select(context =>
                {
                    _ = TryReadString(context, "context", out var id);
                    _ = TryReadString(context, "url", out var url);
                    return new FirefoxContext(id ?? string.Empty, url ?? string.Empty);
                })
                .Where(context => !string.IsNullOrWhiteSpace(context.Id))
                .ToArray();
        }

        public async Task<JsonElement> SendCommand(string method, Action<Utf8JsonWriter>? writeParams = null)
        {
            var id = Interlocked.Increment(ref commandId);
            using var commandStream = new MemoryStream();

            await using (var writer = new Utf8JsonWriter(commandStream))
            {
                writer.WriteStartObject();
                writer.WriteNumber("id", id);
                writer.WriteString("method", method);
                writer.WriteStartObject("params");
                writeParams?.Invoke(writer);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            await socket.SendAsync(commandStream.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
            using var timeout = new CancellationTokenSource(CommandTimeout);

            while (true)
            {
                var message = await ReceiveMessage(timeout.Token);
                using var document = JsonDocument.Parse(message);
                if (!document.RootElement.TryGetProperty("id", out var responseId) ||
                    !responseId.TryGetInt32(out var responseCommandId) ||
                    responseCommandId != id)
                {
                    continue;
                }

                if (document.RootElement.TryGetProperty("type", out var type) &&
                    string.Equals(type.GetString(), "error", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ChromeDevToolsException(ReadProtocolError(document.RootElement));
                }

                return document.RootElement.Clone();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (socket.State is WebSocketState.Open)
            {
                try
                {
                    await SendCommand("session.end");
                }
                catch (ChromeDevToolsException)
                {
                }
                catch (WebSocketException)
                {
                }

                if (socket.State is WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "CMG command complete", CancellationToken.None);
                }
            }

            socket.Dispose();
        }

        private async Task<string> ReceiveMessage(CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];
            using var message = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(message.ToArray());
        }

        private static string ReadProtocolError(JsonElement root)
        {
            if (TryReadString(root, "message", out var message) && !string.IsNullOrWhiteSpace(message))
            {
                return message;
            }

            return TryReadString(root, "error", out var error) && !string.IsNullOrWhiteSpace(error)
                ? error
                : "Firefox WebDriver BiDi protocol error.";
        }
    }

    private readonly record struct ElementRect(double X, double Y, double Width, double Height);

    private sealed record FirefoxContext(string Id, string Url);
}

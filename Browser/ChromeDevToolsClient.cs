using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed class ChromeDevToolsClient
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);

    public string GetElementHtml(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

            foreach (var pageTarget in pageTargets)
            {
                await using var session = await DevToolsSession.Connect(pageTarget);

                await session.ScrollElementIntoView(selector);

                var response = await session.SendCommand("Runtime.evaluate", writer =>
                {
                    writer.WriteString("expression", BuildOuterHtmlExpression(selector));
                    writer.WriteBoolean("returnByValue", true);
                });

                if (TryReadString(response, ["result", "result", "value"], out var html) && html is not null)
                {
                    return html;
                }
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

            foreach (var pageTarget in pageTargets)
            {
                await using var session = await DevToolsSession.Connect(pageTarget);

                await session.ScrollElementIntoView(selector);

                var document = await session.SendCommand("DOM.getDocument");
                var rootNodeId = ReadInt32(document, "result", "root", "nodeId");

                var query = await session.SendCommand("DOM.querySelector", writer =>
                {
                    writer.WriteNumber("nodeId", rootNodeId);
                    writer.WriteString("selector", selector);
                });

                var nodeId = ReadInt32(query, "result", "nodeId");
                if (nodeId is 0)
                {
                    continue;
                }

                var boxModel = await session.SendCommand("DOM.getBoxModel", writer =>
                {
                    writer.WriteNumber("nodeId", nodeId);
                });

                var clip = ElementClip.FromBoxModel(boxModel);
                if (clip.Width <= 0 || clip.Height <= 0)
                {
                    throw new ChromeDevToolsException($"Element '{selector}' has no visible area to screenshot.");
                }

                var screenshot = await session.SendCommand("Page.captureScreenshot", writer =>
                {
                    writer.WriteString("format", "png");
                    writer.WriteStartObject("clip");
                    writer.WriteNumber("x", clip.X);
                    writer.WriteNumber("y", clip.Y);
                    writer.WriteNumber("width", clip.Width);
                    writer.WriteNumber("height", clip.Height);
                    writer.WriteNumber("scale", 1);
                    writer.WriteEndObject();
                });

                if (!TryReadString(screenshot, "result", "data", out var data) || string.IsNullOrWhiteSpace(data))
                {
                    throw new ChromeDevToolsException("Chrome did not return screenshot image data.");
                }

                return Convert.FromBase64String(data);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    private static T Run<T>(Func<Task<T>> action)
    {
        return action().GetAwaiter().GetResult();
    }

    private static async Task<IReadOnlyList<Uri>> GetPageWebSocketDebuggerUrls(string remoteDebuggingUrl)
    {
        using var httpClient = new HttpClient
        {
            Timeout = CommandTimeout
        };

        var targetsJson = await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json");
        using var targets = JsonDocument.Parse(targetsJson);
        var pageTargets = new List<Uri>();

        foreach (var target in targets.RootElement.EnumerateArray())
        {
            if (!TryReadString(target, "type", out var type) ||
                !string.Equals(type, "page", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryReadString(target, "webSocketDebuggerUrl", out var webSocketDebuggerUrl) &&
                !string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
            {
                pageTargets.Add(new Uri(webSocketDebuggerUrl));
            }
        }

        if (pageTargets.Count is 0)
        {
            throw new ChromeDevToolsException("No Chrome page target was available through remote debugging.");
        }

        return pageTargets;
    }

    private static string BuildOuterHtmlExpression(string selector)
    {
        return $"(() => {{ const element = document.querySelector({ToJsonStringLiteral(selector)}); return element ? element.outerHTML : null; }})()";
    }

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

    private sealed class DevToolsSession : IAsyncDisposable
    {
        private readonly ClientWebSocket socket;
        private int commandId;

        private DevToolsSession(ClientWebSocket socket)
        {
            this.socket = socket;
        }

        public static async Task<DevToolsSession> Connect(Uri webSocketDebuggerUrl)
        {
            var socket = new ClientWebSocket();
            await socket.ConnectAsync(webSocketDebuggerUrl, CancellationToken.None);

            return new DevToolsSession(socket);
        }

        public async Task ScrollElementIntoView(string selector)
        {
            await SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", BuildScrollIntoViewExpression(selector));
                writer.WriteBoolean("awaitPromise", true);
            });
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

                if (writeParams is not null)
                {
                    writer.WriteStartObject("params");
                    writeParams(writer);
                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }

            await socket.SendAsync(commandStream.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);

            using var timeout = new CancellationTokenSource(CommandTimeout);

            while (true)
            {
                var response = await ReceiveMessage(timeout.Token);
                using var document = JsonDocument.Parse(response);

                if (!document.RootElement.TryGetProperty("id", out var responseId) ||
                    !responseId.TryGetInt32(out var responseCommandId) ||
                    responseCommandId != id)
                {
                    continue;
                }

                if (document.RootElement.TryGetProperty("error", out var error))
                {
                    throw new ChromeDevToolsException(ReadProtocolError(error));
                }

                return document.RootElement.Clone();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (socket.State is WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "CMG command complete", CancellationToken.None);
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

        private static string BuildScrollIntoViewExpression(string selector)
        {
            return $"(() => {{ const element = document.querySelector({ToJsonStringLiteral(selector)}); if (!element) return false; element.scrollIntoView({{ block: 'center', inline: 'center' }}); return true; }})()";
        }

        private static string ReadProtocolError(JsonElement error)
        {
            if (error.TryGetProperty("message", out var message) &&
                message.ValueKind is JsonValueKind.String)
            {
                return message.GetString() ?? "Chrome DevTools Protocol error.";
            }

            return "Chrome DevTools Protocol error.";
        }
    }

    private readonly record struct ElementClip(double X, double Y, double Width, double Height)
    {
        public static ElementClip FromBoxModel(JsonElement boxModel)
        {
            if (!TryReadElement(boxModel, ["result", "model", "content"], out var content) ||
                content.ValueKind is not JsonValueKind.Array)
            {
                throw new ChromeDevToolsException("Chrome did not return an element box model.");
            }

            var points = content.EnumerateArray().Select(point => point.GetDouble()).ToArray();
            if (points.Length < 8)
            {
                throw new ChromeDevToolsException("Chrome returned an invalid element box model.");
            }

            var xs = new[] { points[0], points[2], points[4], points[6] };
            var ys = new[] { points[1], points[3], points[5], points[7] };
            var x = xs.Min();
            var y = ys.Min();

            return new ElementClip(
                x,
                y,
                xs.Max() - x,
                ys.Max() - y);
        }
    }
}

public sealed class ChromeDevToolsException : Exception
{
    public ChromeDevToolsException(string message)
        : base(message)
    {
    }
}

public sealed class ElementNotFoundException : Exception
{
    public ElementNotFoundException(string selector)
        : base($"No element matched selector '{selector}'.")
    {
    }
}

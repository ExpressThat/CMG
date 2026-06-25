using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private sealed class DevToolsSession : IAsyncDisposable
    {
        private readonly ClientWebSocket socket;
        private int commandId;

        private DevToolsSession(ClientWebSocket socket)
        {
            this.socket = socket;
        }

        public static async Task<DevToolsSession> Connect(Uri webSocketDebuggerUrl, bool enablePage = true)
        {
            var socket = new ClientWebSocket();
            await socket.ConnectAsync(webSocketDebuggerUrl, CancellationToken.None);

            var session = new DevToolsSession(socket);
            if (enablePage)
            {
                await session.EnableAutoDialogHandling();
            }

            return session;
        }

        public async Task EnableAutoDialogHandling()
        {
            await SendCommand("Page.enable");
        }

        public async Task ScrollElementIntoView(string selector)
        {
            await SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", BuildScrollIntoViewExpression(selector));
                writer.WriteBoolean("awaitPromise", true);
            });
        }

        public async Task<JsonElement> SendCommand(string method, Action<Utf8JsonWriter>? writeParams = null, string? sessionId = null)
        {
            var id = Interlocked.Increment(ref commandId);
            using var commandStream = new MemoryStream();

            await using (var writer = new Utf8JsonWriter(commandStream))
            {
                writer.WriteStartObject();
                writer.WriteNumber("id", id);
                writer.WriteString("method", method);
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    writer.WriteString("sessionId", sessionId);
                }

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
                    await HandleProtocolEvent(document.RootElement);
                    continue;
                }

                if (document.RootElement.TryGetProperty("error", out var error))
                {
                    throw new ChromeDevToolsException(ReadProtocolError(error));
                }

                return document.RootElement.Clone();
            }
        }

        private async Task HandleProtocolEvent(JsonElement root)
        {
            if (!TryReadString(root, "method", out var method) ||
                !string.Equals(method, "Page.javascriptDialogOpening", StringComparison.Ordinal))
            {
                return;
            }

            await SendCommandWithoutResponse("Page.handleJavaScriptDialog", writer =>
            {
                writer.WriteBoolean("accept", true);
            });
        }

        private async Task SendCommandWithoutResponse(string method, Action<Utf8JsonWriter>? writeParams = null)
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
            return BrowserDomScripts.ScrollIntoView(selector);
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
}

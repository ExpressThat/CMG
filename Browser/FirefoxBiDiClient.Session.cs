using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
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
}

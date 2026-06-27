using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CMG.E2E.Tests.Support;

public sealed class StaticFixtureServer : IDisposable
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource cancellation = new();
    private readonly Thread loop;

    public StaticFixtureServer(string root)
    {
        Root = root;
        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        loop = new Thread(AcceptLoop) { IsBackground = true, Name = "CMG fixture server" };
        loop.Start();
    }

    public string Root { get; }

    public int Port { get; }

    public string Url(string fileName) => $"http://127.0.0.1:{Port}/{Uri.EscapeDataString(fileName)}";

    public string UrlPath(string path) => $"http://127.0.0.1:{Port}/{path.TrimStart('/')}";

    public string WebSocketUrl(string path) => $"ws://127.0.0.1:{Port}/{path.TrimStart('/')}";

    public void Dispose()
    {
        cancellation.Cancel();
        listener.Stop();
        loop.Join(TimeSpan.FromSeconds(2));

        cancellation.Dispose();
    }

    private void AcceptLoop()
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(_ => Handle(client));
            }
            catch (SocketException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (ObjectDisposedException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void Handle(TcpClient client)
    {
        using (client)
        {
            try
            {
                HandleClient(client);
            }
            catch (Exception exception) when (exception is IOException or SocketException)
            {
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = reader.ReadLine() ?? string.Empty;
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? line;
        while (!string.IsNullOrEmpty(line = reader.ReadLine()))
        {
            var index = line.IndexOf(':');
            if (index > 0)
            {
                headers[line[..index].Trim()] = line[(index + 1)..].Trim();
            }
        }

        if (headers.TryGetValue("Upgrade", out var upgrade) &&
            upgrade.Equals("websocket", StringComparison.OrdinalIgnoreCase))
        {
            AcceptWebSocket(stream, headers);
            return;
        }

        var parts = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var method = parts.Length > 0 ? parts[0] : "GET";
        var target = parts.Length > 1 ? parts[1] : "/index.html";
        var uri = new Uri($"http://fixture.local{target}", UriKind.Absolute);
        if (TryHandleApi(stream, reader, method, uri, headers))
        {
            return;
        }

        var relative = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        var root = Path.GetFullPath(Root);
        var path = Path.GetFullPath(Path.Combine(root, relative));
        var insideRoot = !Path.GetRelativePath(root, path).StartsWith("..", StringComparison.Ordinal);
        if (!insideRoot || !File.Exists(path))
        {
            Write(stream, "404 Not Found", "text/plain", Encoding.UTF8.GetBytes("not found"));
            return;
        }

        Write(stream, "200 OK", ContentType(path), File.ReadAllBytes(path));
    }

    private bool TryHandleApi(
        NetworkStream stream,
        StreamReader reader,
        string method,
        Uri uri,
        IReadOnlyDictionary<string, string> headers)
    {
        if (!uri.AbsolutePath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (uri.AbsolutePath.StartsWith("/api/status/", StringComparison.OrdinalIgnoreCase))
        {
            var status = uri.AbsolutePath.Split('/').Last();
            Write(stream, $"{status} Fixture", "text/plain", Encoding.UTF8.GetBytes($"status {status}"));
            return true;
        }

        if (uri.AbsolutePath.Equals("/api/slow", StringComparison.OrdinalIgnoreCase))
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Write(stream, "200 OK", "text/plain", Encoding.UTF8.GetBytes("slow ok"));
            return true;
        }

        if (!uri.AbsolutePath.Equals("/api/echo", StringComparison.OrdinalIgnoreCase))
        {
            Write(stream, "404 Not Found", "text/plain", Encoding.UTF8.GetBytes("api not found"));
            return true;
        }

        var body = ReadBody(reader, headers);
        var payload = new Dictionary<string, string?>
        {
            ["method"] = method,
            ["query"] = uri.Query.TrimStart('?'),
            ["body"] = body,
            ["contentType"] = headers.GetValueOrDefault("Content-Type"),
            ["xAgent"] = headers.GetValueOrDefault("x-agent"),
            ["authorization"] = DecodeBasicAuth(headers.GetValueOrDefault("Authorization"))
        };
        var json = JsonSerializer.Serialize(payload);
        Write(stream, "200 OK", "application/json", Encoding.UTF8.GetBytes(json), ("x-cmg-api", "fixture"));
        return true;
    }

    private static string ReadBody(StreamReader reader, IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue("Content-Length", out var value) || !int.TryParse(value, out var length) || length <= 0)
        {
            return string.Empty;
        }

        var buffer = new char[length];
        var read = reader.ReadBlock(buffer, 0, length);
        return new string(buffer, 0, read);
    }

    private static string? DecodeBasicAuth(string? value)
    {
        if (value is null || !value.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(value["Basic ".Length..]));
    }

    private void AcceptWebSocket(NetworkStream stream, IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue("Sec-WebSocket-Key", out var key))
        {
            Write(stream, "400 Bad Request", "text/plain", Encoding.UTF8.GetBytes("missing websocket key"));
            return;
        }

        var accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        var response = Encoding.ASCII.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            $"Sec-WebSocket-Accept: {accept}\r\n\r\n");
        stream.Write(response);
        WaitForSocketClose(stream);
    }

    private void WaitForSocketClose(NetworkStream stream)
    {
        var buffer = new byte[128];
        while (!cancellation.IsCancellationRequested)
        {
            if (stream.DataAvailable && stream.Read(buffer) == 0)
            {
                return;
            }

            Thread.Sleep(25);
        }
    }

    private static void Write(Stream stream, string status, string contentType, byte[] body, params (string Name, string Value)[] headers)
    {
        var headerBuilder = new StringBuilder()
            .Append($"HTTP/1.1 {status}\r\n")
            .Append($"Content-Type: {contentType}\r\n")
            .Append($"Content-Length: {body.Length}\r\n")
            .Append("Connection: close\r\n");
        foreach (var extraHeader in headers)
        {
            headerBuilder.Append($"{extraHeader.Name}: {extraHeader.Value}\r\n");
        }

        headerBuilder.Append("\r\n");
        var headerBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
        stream.Write(headerBytes);
        stream.Write(body);
    }

    private static string ContentType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".js" => "text/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            _ => "application/octet-stream"
        };
}

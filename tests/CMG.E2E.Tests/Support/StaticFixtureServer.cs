using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CMG.E2E.Tests.Support;

public sealed class StaticFixtureServer : IDisposable
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource cancellation = new();
    private readonly Task loop;

    public StaticFixtureServer(string root)
    {
        Root = root;
        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        loop = Task.Run(AcceptLoop);
    }

    public string Root { get; }

    public int Port { get; }

    public string Url(string fileName) => $"http://127.0.0.1:{Port}/{Uri.EscapeDataString(fileName)}";

    public void Dispose()
    {
        cancellation.Cancel();
        listener.Stop();
        try
        {
            loop.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
        }

        cancellation.Dispose();
    }

    private async Task AcceptLoop()
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync(cancellation.Token);
                await Handle(client);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (SocketException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task Handle(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = await reader.ReadLineAsync(cancellation.Token) ?? string.Empty;
        while (!string.IsNullOrEmpty(await reader.ReadLineAsync(cancellation.Token)))
        {
        }

        var parts = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var relative = parts.Length > 1 ? Uri.UnescapeDataString(parts[1].TrimStart('/')) : "index.html";
        var root = Path.GetFullPath(Root);
        var path = Path.GetFullPath(Path.Combine(root, relative));
        var insideRoot = !Path.GetRelativePath(root, path).StartsWith("..", StringComparison.Ordinal);
        if (!insideRoot || !File.Exists(path))
        {
            await Write(stream, "404 Not Found", "text/plain", Encoding.UTF8.GetBytes("not found"));
            return;
        }

        await Write(stream, "200 OK", ContentType(path), await File.ReadAllBytesAsync(path, cancellation.Token));
    }

    private static async Task Write(Stream stream, string status, string contentType, byte[] body)
    {
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {status}\r\nContent-Type: {contentType}\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(header);
        await stream.WriteAsync(body);
    }

    private static string ContentType(string path) =>
        Path.GetExtension(path).Equals(".html", StringComparison.OrdinalIgnoreCase)
            ? "text/html; charset=utf-8"
            : "application/octet-stream";
}

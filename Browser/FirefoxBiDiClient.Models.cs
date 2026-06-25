using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private readonly record struct ElementRect(double X, double Y, double Width, double Height);

    private sealed record FirefoxContext(string Id, string Url);
}

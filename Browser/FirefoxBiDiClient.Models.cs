using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private readonly record struct ElementRect(
        double X, double Y, double Width, double Height, double? InteractionX = null, double? InteractionY = null)
    {
        public double CenterX => InteractionX ?? X + Width / 2;
        public double CenterY => InteractionY ?? Y + Height / 2;
    }

    private sealed record FirefoxContext(string Id, string Url);
}

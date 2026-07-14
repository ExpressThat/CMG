using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private readonly record struct ElementClip(
        double X, double Y, double Width, double Height, double? InteractionX = null, double? InteractionY = null)
    {
        public double CenterX => InteractionX ?? X + Width / 2;

        public double CenterY => InteractionY ?? Y + Height / 2;

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

public sealed record ChromePageTab(string Id, string Title, string Url);

public sealed record ElementPoint(double X, double Y);

internal sealed record PageTarget(string Id, string Title, string Url, Uri WebSocketDebuggerUrl);

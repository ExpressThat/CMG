using CMG.Browser;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Runner;

public static class CmgScreenshotMasker
{
    public static byte[] Apply(byte[] png, IReadOnlyList<ElementBox> boxes, string color = "#ff00ff", ElementBox? origin = null)
    {
        if (boxes.Count == 0)
        {
            return png;
        }

        using var image = Image.Load<Rgba32>(png);
        var maskColor = ParseColor(color);
        image.ProcessPixelRows(accessor =>
        {
            foreach (var box in boxes)
            {
                var x = (int)Math.Floor(box.X - (origin?.X ?? 0));
                var y = (int)Math.Floor(box.Y - (origin?.Y ?? 0));
                var width = (int)Math.Ceiling(box.Width);
                var height = (int)Math.Ceiling(box.Height);
                var rectangle = Clamp(new Rectangle(x, y, width, height), image.Width, image.Height);
                for (var rowIndex = rectangle.Top; rowIndex < rectangle.Bottom; rowIndex++)
                {
                    var row = accessor.GetRowSpan(rowIndex);
                    for (var column = rectangle.Left; column < rectangle.Right; column++)
                    {
                        row[column] = maskColor;
                    }
                }
            }
        });

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private static Rgba32 ParseColor(string value)
    {
        return Rgba32.TryParseHex(value.TrimStart('#'), out var rgba)
            ? rgba
            : Color.Magenta.ToPixel<Rgba32>();
    }

    private static Rectangle Clamp(Rectangle rectangle, int width, int height)
    {
        var left = Math.Clamp(rectangle.Left, 0, width);
        var top = Math.Clamp(rectangle.Top, 0, height);
        var right = Math.Clamp(rectangle.Right, 0, width);
        var bottom = Math.Clamp(rectangle.Bottom, 0, height);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }
}

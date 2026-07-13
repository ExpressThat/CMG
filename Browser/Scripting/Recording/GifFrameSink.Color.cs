using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    private string? previousColorProfileSignature;

    public int IccProfileFrameCount { get; private set; }
    public int CicpProfileFrameCount { get; private set; }
    public int GammaMetadataFrameCount { get; private set; }
    public int ColorProfileChangeCount { get; private set; }

    private void TrackColorMetadata(Image<Rgba32> image)
    {
        var icc = image.Metadata.IccProfile;
        var cicp = image.Metadata.CicpProfile;
        var gamma = image.Metadata.GetPngMetadata().Gamma;
        if (icc is not null) IccProfileFrameCount++;
        if (cicp is not null) CicpProfileFrameCount++;
        if (gamma > 0) GammaMetadataFrameCount++;
        var signature = string.Join('|',
            icc is null ? "icc:none" : $"icc:{Convert.ToHexString(SHA256.HashData(icc.ToByteArray()))}",
            cicp is null ? "cicp:none" : $"cicp:{cicp.ColorPrimaries}:{cicp.TransferCharacteristics}:{cicp.MatrixCoefficients}:{cicp.FullRange}",
            gamma > 0 ? $"gamma:{gamma:R}" : "gamma:none");
        if (previousColorProfileSignature is not null && !signature.Equals(previousColorProfileSignature, StringComparison.Ordinal))
            ColorProfileChangeCount++;
        previousColorProfileSignature = signature;
    }

    private bool ApplyColorOptions(Image<Rgba32> image)
    {
        var color = encoding.Color ?? new GifColorOptions();
        var changed = color.ParsedBackground is Color background && FlattenBackground(image, background.ToPixel<Rgba32>());
        if (color.HighContrastPalette)
        {
            image.Mutate(context => context.Contrast(1.18f).Saturate(1.08f));
            changed = true;
        }
        return changed;
    }

    private static bool FlattenBackground(Image<Rgba32> image, Rgba32 background)
    {
        var changed = false;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    if (pixel.A is byte.MaxValue) continue;
                    var alpha = pixel.A;
                    var inverse = byte.MaxValue - alpha;
                    row[x] = new Rgba32(
                        (byte)((pixel.R * alpha + background.R * inverse + 127) / 255),
                        (byte)((pixel.G * alpha + background.G * inverse + 127) / 255),
                        (byte)((pixel.B * alpha + background.B * inverse + 127) / 255),
                        byte.MaxValue);
                    changed = true;
                }
            }
        });
        return changed;
    }
}

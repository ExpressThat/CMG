using System.CommandLine;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;

namespace CMG.Tests;

public sealed class GifEncodingCliOptionsTests
{
    [Fact]
    public void TryParse_MapsEveryWholeRunEncoderOption()
    {
        var options = GifEncodingCliOptions.Build();
        var root = Root(options);
        var directory = Path.Combine(Path.GetTempPath(), "cmg-frames");
        var result = root.Parse(["--gif-dither", "sierra", "--gif-palette", "local", "--gif-colors", "144", "--keep-frames", directory]);

        Assert.True(options.TryParse(result, out var encoding, out var error), error);
        Assert.Equal(GifDitherMode.Sierra, encoding.Dither);
        Assert.Equal(GifPaletteMode.Local, encoding.Palette);
        Assert.Equal(144, encoding.Colors);
        Assert.Equal(Path.GetFullPath(directory), encoding.KeepFramesDirectory);
    }

    [Theory]
    [InlineData("--gif-dither", "sparkle", "dither=")]
    [InlineData("--gif-palette", "shared-ish", "palette=")]
    [InlineData("--gif-colors", "1", "colors=")]
    [InlineData("--gif-colors", "257", "colors=")]
    public void TryParse_RejectsInvalidValues(string option, string value, string expected)
    {
        var options = GifEncodingCliOptions.Build();
        var result = Root(options).Parse([option, value]);

        Assert.False(options.TryParse(result, out _, out var error));
        Assert.Contains(expected, error, StringComparison.Ordinal);
    }

    private static RootCommand Root(GifEncodingCliOptions options) => new()
    {
        Options = { options.Dither, options.Palette, options.Colors, options.KeepFrames }
    };
}

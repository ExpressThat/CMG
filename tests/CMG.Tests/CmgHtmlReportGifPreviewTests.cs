using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgHtmlReportGifPreviewTests
{
    [Fact]
    public void HtmlReport_RendersGifPreviewImage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var report = CmgHtmlReportWriter.Write([TestWithGif(path)]);

        Assert.Contains("gif-previews", report);
        Assert.Contains("<img", report);
        Assert.Contains(new Uri(Path.GetFullPath(path)).AbsoluteUri, report);
        Assert.Contains("GIF: ", report);
    }

    [Fact]
    public void HtmlReport_RendersMultipleGifsInFilmstripAndTestDetail()
    {
        var first = "artifacts\\first.gif";
        var second = "artifacts\\second.gif";
        var report = CmgHtmlReportWriter.Write([TestWithGif($"{first};{second}")]);

        Assert.Equal(4, Count(report, "<figure class=\"gif-preview\">"));
        Assert.Contains("run-filmstrip", report);
        Assert.Contains("artifacts/first.gif", report);
        Assert.Contains("artifacts/second.gif", report);
    }

    [Fact]
    public void HtmlReport_RendersMp4AsVideo()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");
        var report = CmgHtmlReportWriter.Write([TestWithGif(path)]);

        Assert.Contains("<video controls", report);
        Assert.Contains("type=\"video/mp4\"", report);
        Assert.Contains("Open MP4", report);
        Assert.Contains("MP4: ", report);
        Assert.DoesNotContain($"<img src=\"{new Uri(Path.GetFullPath(path)).AbsoluteUri}", report);
    }

    private static CmgTestResult TestWithGif(string gifPath) =>
        new("checkout", "checkout.cmgscript", true, ["PASS 001 click"], null, gifPath, []);

    private static int Count(string value, string fragment)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }
}

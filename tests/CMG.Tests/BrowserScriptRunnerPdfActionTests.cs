using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerPdfActionTests
{
    [Fact]
    public void RunText_PrintPdfWritesFileAndOptions()
    {
        var client = new FakeAutomationClient();
        var path = TempPath();
        var result = Runner().RunText(
            $"printPdf path=\"{Slash(path)}\" landscape=true printBackground=false scale=0.8 format=A4 marginTop=10mm pageRanges=\"1-2,4\" preferCssPageSize=true",
            "debug",
            client);

        Assert.True(result.Success);
        Assert.Equal([1, 2, 3], File.ReadAllBytes(path));
        Assert.Equal(new PdfPrintOptions(true, false, 0.8, "A4", null, null, "10mm", null, null, null, "1-2,4", true), client.LastPdfOptions);
        Assert.Contains(result.StdoutLines, line => line.Contains("PDF", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_PrintPdfRequiresPath()
    {
        var result = Runner().RunText("printPdf", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("requires path", result.Error);
    }

    [Fact]
    public void RunText_PrintPdfRejectsInvalidScale()
    {
        var result = Runner().RunText("printPdf path=\"out.pdf\" scale=0", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("positive number", result.Error);
    }

    [Fact]
    public void RunText_PrintPdfRejectsInvalidFormat()
    {
        var result = Runner().RunText("printPdf path=\"out.pdf\" format=Poster", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("format= must be", result.Error);
    }

    [Fact]
    public void RunText_PrintPdfRejectsInvalidSize()
    {
        var result = Runner().RunText("printPdf path=\"out.pdf\" width=large", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("width= must be a positive size", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static string TempPath() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

    private static string Slash(string path) => path.Replace('\\', '/');
}

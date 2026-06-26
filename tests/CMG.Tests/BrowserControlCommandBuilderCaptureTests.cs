using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderCaptureTests
{
    [Fact]
    public void ToHaveScreenshotCommand_MapsToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse("control capture toHaveScreenshot #dialog --baseline C:\\temp\\baseline.png --output C:\\temp\\actual.png --tolerance 0.05").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(
            "toHaveScreenshot \"#dialog\" baseline=\"C:\\\\temp\\\\baseline.png\" output=\"C:\\\\temp\\\\actual.png\" tolerance=\"0.05\"",
            handler.ScriptLine);
    }

    [Fact]
    public void ExpectScreenshotCommand_MapsVisualOptionsToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control capture expectScreenshot --baseline C:\\temp\\page.png --full-page --mask #clock;#ad --mask-color #000000").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(
            "expectScreenshot baseline=\"C:\\\\temp\\\\page.png\" fullPage=\"true\" mask=\"#clock;#ad\" maskColor=\"#000000\"",
            handler.ScriptLine);
    }

    [Fact]
    public void PrintPdfCommand_MapsAdvancedOptionsToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control capture printPdf --path C:\\temp\\page.pdf --format A4 --margin-top 10mm --margin-bottom 0.5in --page-ranges 1-2,4 --prefer-css-page-size").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(
            "printPdf path=\"C:\\\\temp\\\\page.pdf\" format=\"A4\" marginTop=\"10mm\" marginBottom=\"0.5in\" pageRanges=\"1-2,4\" preferCssPageSize=\"true\"",
            handler.ScriptLine);
    }

    [Fact]
    public void ScreenshotCommand_MapsImageOptionsToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse("control capture screenshot #card --type jpeg --quality 75 --omit-background").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("screenshot \"#card\" type=\"jpeg\" quality=\"75\" omitBackground=\"true\"", handler.ScriptLine);
    }

    [Fact]
    public void ScreenshotPageCommand_MapsImageOptionsToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse("control capture screenshotPage --full-page --type jpg --quality 80").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("screenshotPage type=\"jpg\" quality=\"80\" fullPage=\"true\"", handler.ScriptLine);
    }

    private static RootCommand BuildRoot(CapturingBrowserControlCommandHandler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome);
        root.Options.Add(edge);
        root.Options.Add(firefox);
        root.Subcommands.Add(new BrowserControlCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class CapturingBrowserControlCommandHandler : IBrowserControlCommandHandler
    {
        public BrowserKind BrowserKind { get; private set; }

        public string? ScriptLine { get; private set; }

        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif) => 0;

        public int ValidateScript(string file) => 0;

        public int RunScriptAction(BrowserKind browserKind, string scriptLine)
        {
            BrowserKind = browserKind;
            ScriptLine = scriptLine;
            return 0;
        }
    }
}

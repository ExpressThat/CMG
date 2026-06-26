using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildPrintPdfCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var path = new Option<FileInfo>("--path")
        {
            Description = "Output PDF path.",
            Required = true
        };
        var landscape = CliStringOption("--landscape", "Use landscape orientation: true or false.");
        var printBackground = CliStringOption("--print-background", "Print backgrounds: true or false. Default is true.");
        var scale = new Option<double?>("--scale") { Description = "Positive print scale. Default is 1." };
        var command = new Command(name, "Print the current page to PDF.") { path, landscape, printBackground, scale };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, [], CompactOptions([
                StringOption("path", parseResult.GetValue(path)?.FullName),
                StringOption("landscape", parseResult.GetValue(landscape)),
                StringOption("printBackground", parseResult.GetValue(printBackground)),
                StringOption("scale", parseResult.GetValue(scale)?.ToString())
            ]))));

        return command;
    }

    private Command BuildExpectScreenshotCommand(BrowserSelectionOptions browserOptions)
    {
        return BuildExpectScreenshotCommand(browserOptions, "expectScreenshot");
    }

    private Command BuildExpectScreenshotCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var selector = OptionalTextArgument("selector", "Optional selector. Omit to compare the page viewport.");
        var baseline = new Option<FileInfo>("--baseline")
        {
            Description = "Baseline PNG path.",
            Required = true
        };
        var output = new Option<FileInfo?>("--output") { Description = "Actual PNG output path. Default is actual.png." };
        var tolerance = new Option<double?>("--tolerance") { Description = "Allowed normalized diff. Default is 0." };
        var command = new Command(name, "Compare an element or page screenshot to a baseline.") { selector, baseline, output, tolerance };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, OptionalArgument(parseResult, selector), CompactOptions([
                StringOption("baseline", parseResult.GetValue(baseline)?.FullName),
                StringOption("output", parseResult.GetValue(output)?.FullName),
                StringOption("tolerance", parseResult.GetValue(tolerance)?.ToString())
            ]))));

        return command;
    }
}

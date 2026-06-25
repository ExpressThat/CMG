using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildGetElementCommand(BrowserSelectionOptions browserOptions)
    {
        var selectorArgument = new Argument<string>("selector")
        {
            Description = "CSS selector or CMG rich locator for the element."
        };

        var htmlOption = new Option<bool>("--html")
        {
            Description = "Return the selected element HTML."
        };

        var screenshotOption = new Option<bool>("--screenshot")
        {
            Description = "Return a PNG screenshot of the selected element as base64."
        };
        screenshotOption.Aliases.Add("--sscreenshot");

        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Write the screenshot PNG to this file instead of stdout."
        };

        var command = new Command("getElement", "Return HTML or a screenshot for a selected element.")
        {
            selectorArgument,
            htmlOption,
            screenshotOption,
            outputOption
        };

        command.SetAction(parseResult =>
        {
            var selector = parseResult.GetValue(selectorArgument) ?? string.Empty;
            var html = parseResult.GetValue(htmlOption);
            var screenshot = parseResult.GetValue(screenshotOption);
            var output = parseResult.GetValue(outputOption);

            return browserControlCommandHandler.GetElement(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), selector, html, screenshot, output);
        });

        return command;
    }

    private Command BuildScriptCommand(BrowserSelectionOptions browserOptions)
    {
        var fileOption = new Option<string>("--file")
        {
            Description = "Path to a .cmgscript file, or '-' to read from stdin.",
            Required = true
        };

        var gifOption = new Option<FileInfo?>("--gif")
        {
            Description = "Write an animated GIF recording of the script to this path."
        };

        var command = new Command("script", "Run a .cmgscript browser automation script.")
        {
            fileOption,
            gifOption
        };

        command.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileOption) ?? string.Empty;
            var gif = parseResult.GetValue(gifOption);

            return browserControlCommandHandler.RunScript(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), file, gif);
        });

        return command;
    }
}

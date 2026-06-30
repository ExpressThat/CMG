using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

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

            return browserControlCommandHandler.GetElement(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), selector, html, screenshot, output);
        });

        return command;
    }

    private Command BuildValidateScriptCommand()
    {
        var fileOption = new Option<string>("--file")
        {
            Description = "Path to a .cmgscript file, or '-' to read from stdin."
        };
        var inlineOption = new Option<string>("--inline")
        {
            Description = "Inline .cmgscript text to validate."
        };

        var command = new Command("validateScript", "Validate a .cmgscript browser automation script without running it.")
        {
            fileOption,
            inlineOption
        };

        command.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileOption) ?? string.Empty;
            var inline = parseResult.GetValue(inlineOption);
            if (!ValidateScriptInput(file, inline))
            {
                return 1;
            }

            return inline is null
                ? browserControlCommandHandler.ValidateScript(file)
                : browserControlCommandHandler.ValidateInlineScript(inline);
        });

        return command;
    }

    private static bool ValidateScriptInput(string file, string? inline)
    {
        var hasFile = !string.IsNullOrWhiteSpace(file);
        var hasInline = inline is not null;
        if (hasFile == hasInline)
        {
            Console.Error.WriteLine("Specify exactly one script input: --file or --inline.");
            return false;
        }

        return true;
    }
}

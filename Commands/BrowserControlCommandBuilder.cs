using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed class BrowserControlCommandBuilder
{
    private readonly IBrowserControlCommandHandler browserControlCommandHandler;

    public BrowserControlCommandBuilder(IBrowserControlCommandHandler browserControlCommandHandler)
    {
        this.browserControlCommandHandler = browserControlCommandHandler;
    }

    public Command Build()
    {
        var command = new Command("control", "Browser interaction and page control commands.");

        command.SetAction(_ =>
        {
            Console.Error.WriteLine("No browser control command was provided.");
            Console.Error.WriteLine("Run 'cmg browser control --help' to see available commands.");

            return 1;
        });

        command.Subcommands.Add(BuildGetElementCommand());
        command.Subcommands.Add(BuildScriptCommand());

        return command;
    }

    private Command BuildGetElementCommand()
    {
        var selectorArgument = new Argument<string>("selector")
        {
            Description = "CSS selector for the element."
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

            return browserControlCommandHandler.GetElement(selector, html, screenshot, output);
        });

        return command;
    }

    private Command BuildScriptCommand()
    {
        var fileOption = new Option<string>("--file")
        {
            Description = "Path to a .cmgscript file, or '-' to read from stdin.",
            Required = true
        };

        var command = new Command("script", "Run a .cmgscript browser automation script.")
        {
            fileOption
        };

        command.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileOption) ?? string.Empty;

            return browserControlCommandHandler.RunScript(file);
        });

        return command;
    }
}

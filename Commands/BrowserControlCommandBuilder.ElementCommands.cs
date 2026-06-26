using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting;

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
        var traceOption = new Option<FileInfo?>("--trace")
        {
            Description = "Write a CMG script trace JSON file for the run."
        };
        var timeoutOption = new Option<int?>("--timeout")
        {
            Description = "Default timeout in milliseconds for timeout-capable actions."
        };
        var navigationTimeoutOption = new Option<int?>("--navigation-timeout")
        {
            Description = "Default timeout in milliseconds for navigation actions."
        };
        var assertionTimeoutOption = new Option<int?>("--assertion-timeout")
        {
            Description = "Default timeout in milliseconds for assertion actions."
        };
        var variableOption = new Option<string[]>("--var")
        {
            Description = "Initial script variable as name=value. Can be repeated."
        };
        var envOption = new Option<string[]>("--env")
        {
            Description = "Alias for --var, useful for agent-provided environment values."
        };

        var command = new Command("script", "Run a .cmgscript browser automation script.")
        {
            fileOption,
            gifOption,
            traceOption,
            timeoutOption,
            navigationTimeoutOption,
            assertionTimeoutOption,
            variableOption,
            envOption
        };

        command.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileOption) ?? string.Empty;
            var gif = parseResult.GetValue(gifOption);
            var trace = parseResult.GetValue(traceOption);
            var timeouts = new ScriptTimeoutOptions(
                parseResult.GetValue(timeoutOption),
                parseResult.GetValue(navigationTimeoutOption),
                parseResult.GetValue(assertionTimeoutOption));
            var variableValues = (parseResult.GetValue(variableOption) ?? [])
                .Concat(parseResult.GetValue(envOption) ?? []);
            if (!VariableOptionParser.TryParse(variableValues, out var variables, out var error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }

            return browserControlCommandHandler.RunScript(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                file,
                gif,
                trace,
                timeouts,
                variables);
        });

        return command;
    }

    private Command BuildValidateScriptCommand()
    {
        var fileOption = new Option<string>("--file")
        {
            Description = "Path to a .cmgscript file, or '-' to read from stdin.",
            Required = true
        };

        var command = new Command("validateScript", "Validate a .cmgscript browser automation script without running it.")
        {
            fileOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.ValidateScript(parseResult.GetValue(fileOption) ?? string.Empty));

        return command;
    }
}

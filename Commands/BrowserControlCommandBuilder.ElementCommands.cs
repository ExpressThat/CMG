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

    private Command BuildScriptCommand(BrowserSelectionOptions browserOptions)
    {
        var fileOption = new Option<string>("--file")
        {
            Description = "Path to a .cmgscript file, or '-' to read from stdin."
        };
        var inlineOption = new Option<string>("--inline")
        {
            Description = "Inline .cmgscript text to run."
        };

        var gifOption = new Option<FileInfo?>("--gif")
        {
            Description = "Write an animated GIF recording of the script to this path."
        };
        var gifQualityOption = new Option<string>("--gif-quality")
        {
            Description = "GIF quality: highest, high, medium, or low.",
            DefaultValueFactory = _ => "highest"
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
        var baseUrlOption = new Option<string?>("--base-url")
        {
            Description = "Base URL used to resolve relative navigation targets."
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
            inlineOption,
            gifOption,
            gifQualityOption,
            traceOption,
            timeoutOption,
            navigationTimeoutOption,
            assertionTimeoutOption,
            baseUrlOption,
            variableOption,
            envOption
        };

        command.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileOption) ?? string.Empty;
            var inline = parseResult.GetValue(inlineOption);
            if (!ValidateScriptInput(file, inline))
            {
                return 1;
            }

            var gif = parseResult.GetValue(gifOption);
            if (!GifQualityParser.TryParse(parseResult.GetValue(gifQualityOption), out var gifQuality))
            {
                Console.Error.WriteLine($"--gif-quality must be one of: {GifQualityParser.Values}.");
                return 1;
            }

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

            var browserKind = CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions);
            var port = CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions);
            return inline is null
                ? browserControlCommandHandler.RunScript(browserKind, port, file, gif, trace, timeouts, parseResult.GetValue(baseUrlOption), variables, gifQuality)
                : browserControlCommandHandler.RunInlineScript(browserKind, port, inline, gif, trace, timeouts, parseResult.GetValue(baseUrlOption), variables, gifQuality);
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

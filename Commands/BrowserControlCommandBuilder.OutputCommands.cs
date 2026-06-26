using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildCaptureGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("capture", "Element and page capture commands.");

        command.Subcommands.Add(BuildGetElementCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "html", "Print an element's outer HTML."));
        command.Subcommands.Add(BuildScreenshotCommand(browserOptions));
        command.Subcommands.Add(BuildScreenshotPageCommand(browserOptions));
        command.Subcommands.Add(BuildPrintPdfCommand(browserOptions, "printPdf"));
        command.Subcommands.Add(BuildPrintPdfCommand(browserOptions, "pdf"));
        command.Subcommands.Add(BuildExpectScreenshotCommand(browserOptions));
        command.Subcommands.Add(BuildExpectScreenshotCommand(browserOptions, "toHaveScreenshot"));
        return command;
    }

    private Command BuildShowMessageBarCommand(BrowserSelectionOptions browserOptions)
    {
        var messageArgument = new Argument<string>("message")
        {
            Description = "Message to show in a fixed bar at the top of the page."
        };

        var command = new Command("showMessageBar", "Inject or update a fixed message bar at the top of the page.")
        {
            messageArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("showMessageBar", parseResult.GetValue(messageArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildDelayCommand(BrowserSelectionOptions browserOptions)
    {
        var millisecondsArgument = new Argument<int>("milliseconds")
        {
            Description = "Delay duration in milliseconds."
        };

        var command = new Command("delay", "Pause execution for a duration.")
        {
            millisecondsArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("delay", parseResult.GetValue(millisecondsArgument).ToString())));

        return command;
    }

    private Command BuildScreenshotCommand(BrowserSelectionOptions browserOptions)
    {
        var selectorArgument = CreateSelectorArgument();
        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Write the PNG screenshot to this file instead of stdout data URL."
        };

        var command = new Command("screenshot", "Capture an element screenshot.")
        {
            selectorArgument,
            outputOption
        };

        command.SetAction(parseResult =>
        {
            var options = ToOutputOptions(parseResult.GetValue(outputOption));
            return browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "screenshot",
                [parseResult.GetValue(selectorArgument) ?? string.Empty],
                options));
        });

        return command;
    }

    private Command BuildScreenshotPageCommand(BrowserSelectionOptions browserOptions)
    {
        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Write the PNG screenshot to this file instead of stdout data URL."
        };
        var fullPageOption = new Option<bool>("--full-page")
        {
            Description = "Capture the full scrollable page instead of only the current viewport."
        };

        var command = new Command("screenshotPage", "Capture a page screenshot.")
        {
            outputOption,
            fullPageOption
        };

        command.SetAction(parseResult =>
        {
            var options = ToOutputOptions(parseResult.GetValue(outputOption)).ToList();
            if (parseResult.GetValue(fullPageOption))
            {
                options.Add(("fullPage", "true"));
            }

            return browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("screenshotPage", [], options));
        });

        return command;
    }

    private Command BuildAssertTextCommand(BrowserSelectionOptions browserOptions)
    {
        var selectorArgument = CreateSelectorArgument();
        var expectedArgument = new Argument<string>("expected")
        {
            Description = "Expected text fragment."
        };
        var timeoutOption = new Option<int?>("--timeout")
        {
            Description = "Timeout in milliseconds."
        };

        var command = new Command("assertText", "Assert that an element contains text.")
        {
            selectorArgument,
            expectedArgument,
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "assertText",
                [
                    parseResult.GetValue(selectorArgument) ?? string.Empty,
                    parseResult.GetValue(expectedArgument) ?? string.Empty
                ],
                TimeoutOptions(parseResult, timeoutOption))));

        return command;
    }

    private Command BuildEvaluateCommand(BrowserSelectionOptions browserOptions)
    {
        var expressionArgument = new Argument<string>("expression")
        {
            Description = "JavaScript expression to evaluate."
        };

        var command = new Command("evaluate", "Evaluate JavaScript in the primary page.")
        {
            expressionArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("evaluate", parseResult.GetValue(expressionArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildSetViewportCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var widthOption = new Option<int>("--width")
        {
            Description = "Viewport width in CSS pixels.",
            Required = true
        };
        var heightOption = new Option<int>("--height")
        {
            Description = "Viewport height in CSS pixels.",
            Required = true
        };
        var scaleOption = new Option<double?>("--device-scale-factor")
        {
            Description = "Device scale factor. Default is 1."
        };
        var mobileOption = new Option<bool>("--mobile")
        {
            Description = "Use mobile viewport metrics."
        };
        var touchOption = new Option<bool>("--touch")
        {
            Description = "Enable touch viewport hints."
        };

        var command = new Command(name, "Set viewport dimensions.")
        {
            widthOption,
            heightOption,
            scaleOption,
            mobileOption,
            touchOption
        };

        command.SetAction(parseResult =>
        {
            var options = new List<(string Key, string Value)>
            {
                ("width", parseResult.GetValue(widthOption).ToString()),
                ("height", parseResult.GetValue(heightOption).ToString())
            };
            var scale = parseResult.GetValue(scaleOption);
            if (scale is not null)
            {
                options.Add(("deviceScaleFactor", scale.Value.ToString()));
            }
            if (parseResult.GetValue(mobileOption))
            {
                options.Add(("isMobile", "true"));
            }
            if (parseResult.GetValue(touchOption))
            {
                options.Add(("hasTouch", "true"));
            }

            return browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("setViewport", [], options));
        });

        return command;
    }
}

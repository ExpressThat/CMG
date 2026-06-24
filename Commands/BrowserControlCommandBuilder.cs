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
        command.Subcommands.Add(BuildNavigateCommand());
        command.Subcommands.Add(BuildWaitForElementCommand());
        command.Subcommands.Add(BuildSelectorCommand("click", "Click an element."));
        command.Subcommands.Add(BuildTypeCommand());
        command.Subcommands.Add(BuildSelectorCommand("clear", "Clear an input-like element."));
        command.Subcommands.Add(BuildPressCommand());
        command.Subcommands.Add(BuildSelectorCommand("hover", "Hover an element."));
        command.Subcommands.Add(BuildSelectorCommand("scrollIntoView", "Scroll an element into view."));
        command.Subcommands.Add(BuildSelectCommand());
        command.Subcommands.Add(BuildShowMessageBarCommand());
        command.Subcommands.Add(BuildDelayCommand());
        command.Subcommands.Add(BuildSelectorCommand("html", "Print an element's outer HTML."));
        command.Subcommands.Add(BuildScreenshotCommand());
        command.Subcommands.Add(BuildScreenshotPageCommand());
        command.Subcommands.Add(BuildAssertTextCommand());
        command.Subcommands.Add(BuildEvaluateCommand());
        command.Subcommands.Add(BuildSetViewportCommand());
        command.Subcommands.Add(BuildDragAndDropCommand());
        command.Subcommands.Add(BuildNoArgumentCommand("listTabs", "List available Chrome page targets."));
        command.Subcommands.Add(BuildIndexedCommand("activateTab", "Activate a Chrome page target by index."));
        command.Subcommands.Add(BuildIndexedCommand("closeTab", "Close a Chrome page target by index."));
        command.Subcommands.Add(BuildSetCommand());

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

            return browserControlCommandHandler.RunScript(file, gif);
        });

        return command;
    }

    private Command BuildNavigateCommand()
    {
        var targetArgument = new Argument<string>("target")
        {
            Description = "URL, data URL, or local file path."
        };

        var command = new Command("navigate", "Navigate the primary page target.")
        {
            targetArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine("navigate", parseResult.GetValue(targetArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildWaitForElementCommand()
    {
        var selectorArgument = CreateSelectorArgument();
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };

        var command = new Command("waitForElement", "Wait until an element exists.")
        {
            selectorArgument,
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "waitForElement",
                [parseResult.GetValue(selectorArgument) ?? string.Empty],
                [("timeout", parseResult.GetValue(timeoutOption).ToString())])));

        return command;
    }

    private Command BuildSelectorCommand(string name, string description)
    {
        var selectorArgument = CreateSelectorArgument();
        var command = new Command(name, description)
        {
            selectorArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(name, parseResult.GetValue(selectorArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildTypeCommand()
    {
        var selectorArgument = CreateSelectorArgument();
        var textArgument = new Argument<string>("text")
        {
            Description = "Text to append to the element value."
        };

        var command = new Command("type", "Type text into an element.")
        {
            selectorArgument,
            textArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "type",
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(textArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildPressCommand()
    {
        var keyArgument = new Argument<string>("key")
        {
            Description = "Key name to press, such as Enter or Escape."
        };

        var command = new Command("press", "Press a keyboard key.")
        {
            keyArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine("press", parseResult.GetValue(keyArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildSelectCommand()
    {
        var selectorArgument = CreateSelectorArgument();
        var valueArgument = new Argument<string>("value")
        {
            Description = "Value to select."
        };

        var command = new Command("select", "Set a select-like element value.")
        {
            selectorArgument,
            valueArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "select",
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildShowMessageBarCommand()
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
            browserControlCommandHandler.RunScriptAction(ToScriptLine("showMessageBar", parseResult.GetValue(messageArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildDelayCommand()
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
            browserControlCommandHandler.RunScriptAction(ToScriptLine("delay", parseResult.GetValue(millisecondsArgument).ToString())));

        return command;
    }

    private Command BuildScreenshotCommand()
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
            return browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "screenshot",
                [parseResult.GetValue(selectorArgument) ?? string.Empty],
                options));
        });

        return command;
    }

    private Command BuildScreenshotPageCommand()
    {
        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Write the PNG screenshot to this file instead of stdout data URL."
        };

        var command = new Command("screenshotPage", "Capture a full viewport screenshot.")
        {
            outputOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine("screenshotPage", [], ToOutputOptions(parseResult.GetValue(outputOption)))));

        return command;
    }

    private Command BuildAssertTextCommand()
    {
        var selectorArgument = CreateSelectorArgument();
        var expectedArgument = new Argument<string>("expected")
        {
            Description = "Expected text fragment."
        };

        var command = new Command("assertText", "Assert that an element contains text.")
        {
            selectorArgument,
            expectedArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "assertText",
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(expectedArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildEvaluateCommand()
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
            browserControlCommandHandler.RunScriptAction(ToScriptLine("evaluate", parseResult.GetValue(expressionArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildSetViewportCommand()
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

        var command = new Command("setViewport", "Set viewport dimensions.")
        {
            widthOption,
            heightOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "setViewport",
                [],
                [
                    ("width", parseResult.GetValue(widthOption).ToString()),
                    ("height", parseResult.GetValue(heightOption).ToString())
                ])));

        return command;
    }

    private Command BuildDragAndDropCommand()
    {
        var sourceArgument = new Argument<string>("sourceSelector")
        {
            Description = "CSS selector for the drag source."
        };
        var targetArgument = new Argument<string>("targetSelector")
        {
            Description = "CSS selector for the drop target."
        };

        var command = new Command("dragAndDrop", "Drag one element onto another.")
        {
            sourceArgument,
            targetArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "dragAndDrop",
                parseResult.GetValue(sourceArgument) ?? string.Empty,
                parseResult.GetValue(targetArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildNoArgumentCommand(string name, string description)
    {
        var command = new Command(name, description);

        command.SetAction(_ => browserControlCommandHandler.RunScriptAction(name));

        return command;
    }

    private Command BuildIndexedCommand(string name, string description)
    {
        var indexOption = new Option<int>("--index")
        {
            Description = "Zero-based tab index.",
            Required = true
        };

        var command = new Command(name, description)
        {
            indexOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(name, [], [("index", parseResult.GetValue(indexOption).ToString())])));

        return command;
    }

    private Command BuildSetCommand()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "Variable name."
        };
        var valueArgument = new Argument<string>("value")
        {
            Description = "Variable value."
        };

        var command = new Command("set", "Set a script variable for this one action invocation.")
        {
            nameArgument,
            valueArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(ToScriptLine(
                "set",
                parseResult.GetValue(nameArgument) ?? string.Empty,
                parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }

    private static Argument<string> CreateSelectorArgument()
    {
        return new Argument<string>("selector")
        {
            Description = "CSS selector."
        };
    }

    private static IReadOnlyList<(string Key, string Value)> ToOutputOptions(FileInfo? output)
    {
        return output is null ? [] : [("output", output.FullName)];
    }

    private static string ToScriptLine(string action, string argument)
    {
        return ToScriptLine(action, [argument], []);
    }

    private static string ToScriptLine(string action, string firstArgument, string secondArgument)
    {
        return ToScriptLine(action, [firstArgument, secondArgument], []);
    }

    private static string ToScriptLine(
        string action,
        IReadOnlyList<string> arguments,
        IReadOnlyList<(string Key, string Value)> options)
    {
        var parts = new List<string> { action };
        parts.AddRange(arguments.Select(QuoteScriptValue));
        parts.AddRange(options.Select(option => $"{option.Key}={QuoteScriptValue(option.Value)}"));

        return string.Join(' ', parts);
    }

    private static string QuoteScriptValue(string value)
    {
        return $"\"{value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)}\"";
    }
}

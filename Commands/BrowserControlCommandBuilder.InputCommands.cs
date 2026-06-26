using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildInputGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("input", "Pointer, keyboard, and form input commands.");

        command.Subcommands.Add(BuildWaitForElementCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "click", "Click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "doubleClick", "Double-click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "rightClick", "Right-click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "tap", "Tap an element with touch-style events."));
        command.Subcommands.Add(BuildTypeCommand(browserOptions));
        command.Subcommands.Add(BuildFillCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "clear", "Clear an input-like element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "check", "Check a checkbox-like element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "uncheck", "Uncheck a checkbox-like element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "focus", "Focus an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "blur", "Blur an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "selectText", "Select text inside an element."));
        command.Subcommands.Add(BuildPressCommand(browserOptions));
        command.Subcommands.Add(BuildKeyboardCommand(browserOptions, "keyDown", "Dispatch a keydown event."));
        command.Subcommands.Add(BuildKeyboardCommand(browserOptions, "keyUp", "Dispatch a keyup event."));
        command.Subcommands.Add(BuildKeyboardCommand(browserOptions, "insertText", "Insert text at the active element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "hover", "Hover an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "scrollIntoView", "Scroll an element into view."));
        command.Subcommands.Add(BuildSelectCommand(browserOptions));
        command.Subcommands.Add(BuildDragAndDropCommand(browserOptions));
        command.Subcommands.Add(BuildMouseGroup(browserOptions));
        command.Subcommands.Add(BuildScrollGroup(browserOptions));
        command.Subcommands.Add(BuildClipboardGroup(browserOptions));
        command.Subcommands.Add(BuildDispatchEventCommand(browserOptions));

        return command;
    }

    private Command BuildNavigateCommand(BrowserSelectionOptions browserOptions)
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("navigate", parseResult.GetValue(targetArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildWaitForElementCommand(BrowserSelectionOptions browserOptions)
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "waitForElement",
                [parseResult.GetValue(selectorArgument) ?? string.Empty],
                [("timeout", parseResult.GetValue(timeoutOption).ToString())])));

        return command;
    }

    private Command BuildSelectorCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var selectorArgument = CreateSelectorArgument();
        var command = new Command(name, description)
        {
            selectorArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(name, parseResult.GetValue(selectorArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildTypeCommand(BrowserSelectionOptions browserOptions)
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "type",
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(textArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildFillCommand(BrowserSelectionOptions browserOptions)
    {
        var selectorArgument = CreateSelectorArgument();
        var textArgument = new Argument<string>("text")
        {
            Description = "Text to set as the element value."
        };

        var command = new Command("fill", "Replace an input-like element value.")
        {
            selectorArgument,
            textArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "fill",
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(textArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildPressCommand(BrowserSelectionOptions browserOptions)
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("press", parseResult.GetValue(keyArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildKeyboardCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var valueArgument = new Argument<string>("value")
        {
            Description = name.Equals("insertText", StringComparison.Ordinal) ? "Text to insert." : "Key name."
        };

        var command = new Command(name, description)
        {
            valueArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildSelectCommand(BrowserSelectionOptions browserOptions)
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "select",
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }
}

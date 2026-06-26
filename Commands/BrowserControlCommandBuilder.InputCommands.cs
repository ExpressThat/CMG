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
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "dblclick", "Double-click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "doubleClick", "Double-click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "rightClick", "Right-click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "contextClick", "Right-click an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "tap", "Tap an element with touch-style events."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "touchTap", "Tap an element with touch-style events."));
        command.Subcommands.Add(BuildTypeCommand(browserOptions));
        command.Subcommands.Add(BuildTextInputCommand(browserOptions, "pressSequentially", "Type text into an element using sequential key presses."));
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
        command.Subcommands.Add(BuildSelectLikeCommand(browserOptions, "selectOption", "Set a select-like element value."));
        command.Subcommands.Add(BuildDragAndDropCommand(browserOptions, "dragAndDrop"));
        command.Subcommands.Add(BuildDragAndDropCommand(browserOptions, "dragTo"));
        command.Subcommands.Add(BuildKeyboardShortcutCommand(browserOptions, "shortcut"));
        command.Subcommands.Add(BuildKeyboardShortcutCommand(browserOptions, "hotkey"));
        command.Subcommands.Add(BuildKeyboardShortcutCommand(browserOptions, "keyboardShortcut"));
        command.Subcommands.Add(BuildMouseGroup(browserOptions));
        command.Subcommands.Add(BuildScrollGroup(browserOptions));
        command.Subcommands.Add(BuildClipboardGroup(browserOptions));
        command.Subcommands.Add(BuildDispatchEventCommand(browserOptions));
        command.Subcommands.Add(BuildUploadFilesCommand(browserOptions));
        command.Subcommands.Add(BuildUploadFilesCommand(browserOptions, "setInputFiles"));
        command.Subcommands.Add(BuildUploadFilesCommand(browserOptions, "selectFile"));

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
        return BuildTextInputCommand(browserOptions, "type", "Type text into an element.");
    }

    private Command BuildTextInputCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var selectorArgument = CreateSelectorArgument();
        var textArgument = new Argument<string>("text")
        {
            Description = "Text to append to the element value."
        };

        var command = new Command(name, description)
        {
            selectorArgument,
            textArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
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

    private Command BuildSelectCommand(BrowserSelectionOptions browserOptions)
    {
        return BuildSelectLikeCommand(browserOptions, "select", "Set a select-like element value.");
    }

    private Command BuildSelectLikeCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var selectorArgument = CreateSelectorArgument();
        var valueArgument = new Argument<string>("value")
        {
            Description = "Value to select."
        };

        var command = new Command(name, description)
        {
            selectorArgument,
            valueArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                parseResult.GetValue(selectorArgument) ?? string.Empty,
                parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }
}

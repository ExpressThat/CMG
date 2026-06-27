using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildInputGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("input", "Pointer, keyboard, and form input commands.");

        command.Subcommands.Add(BuildWaitForElementCommand(browserOptions));
        command.Subcommands.Add(BuildClickCommand(browserOptions));
        command.Subcommands.Add(BuildMouseClickVariantCommand(browserOptions, "dblclick", "Double-click an element."));
        command.Subcommands.Add(BuildMouseClickVariantCommand(browserOptions, "doubleClick", "Double-click an element."));
        command.Subcommands.Add(BuildMouseClickVariantCommand(browserOptions, "rightClick", "Right-click an element."));
        command.Subcommands.Add(BuildMouseClickVariantCommand(browserOptions, "contextClick", "Right-click an element."));
        command.Subcommands.Add(BuildTapCommand(browserOptions, "tap"));
        command.Subcommands.Add(BuildTapCommand(browserOptions, "touchTap"));
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
        command.Subcommands.Add(BuildHoverCommand(browserOptions));
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(name, parseResult.GetValue(selectorArgument) ?? string.Empty)));

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
        var delay = CliIntOption("--delay", "Delay in milliseconds between typed characters.");

        var command = new Command(name, description)
        {
            selectorArgument,
            textArgument,
            delay
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
                name,
                [parseResult.GetValue(selectorArgument) ?? string.Empty, parseResult.GetValue(textArgument) ?? string.Empty],
                CompactOptions([IntOption("delay", parseResult.GetValue(delay))]))));

        return command;
    }

    private Command BuildClickCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var button = CliStringOption("--button", "Mouse button: left, right, or middle.");
        var clickCount = CliIntOption("--click-count", "Number of clicks to dispatch.");
        var delay = CliIntOption("--delay", "Delay in milliseconds between repeated clicks.");
        var modifiers = CliStringOption("--modifiers", "Comma- or plus-separated modifiers: Alt, Control, Meta, Shift.");
        var x = CliIntOption("--x", "X offset inside the element.");
        var y = CliIntOption("--y", "Y offset inside the element.");
        var command = new Command("click", "Click an element.") { selector, button, clickCount, delay, modifiers, x, y };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine("click", [parseResult.GetValue(selector) ?? string.Empty], CompactOptions([
                StringOption("button", parseResult.GetValue(button)),
                IntOption("clickCount", parseResult.GetValue(clickCount)),
                IntOption("delay", parseResult.GetValue(delay)),
                StringOption("modifiers", parseResult.GetValue(modifiers)),
                IntOption("x", parseResult.GetValue(x)),
                IntOption("y", parseResult.GetValue(y))
            ]))));
        return command;
    }

    private Command BuildHoverCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var modifiers = CliStringOption("--modifiers", "Comma- or plus-separated modifiers: Alt, Control, Meta, Shift.");
        var x = CliIntOption("--x", "X offset inside the element.");
        var y = CliIntOption("--y", "Y offset inside the element.");
        var command = new Command("hover", "Hover an element.") { selector, modifiers, x, y };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine("hover", [parseResult.GetValue(selector) ?? string.Empty], CompactOptions([
                StringOption("modifiers", parseResult.GetValue(modifiers)),
                IntOption("x", parseResult.GetValue(x)),
                IntOption("y", parseResult.GetValue(y))
            ]))));
        return command;
    }

    private Command BuildMouseClickVariantCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var selector = CreateSelectorArgument();
        var modifiers = CliStringOption("--modifiers", "Comma- or plus-separated modifiers: Alt, Control, Meta, Shift.");
        var x = CliIntOption("--x", "X offset inside the element.");
        var y = CliIntOption("--y", "Y offset inside the element.");
        var command = new Command(name, description) { selector, modifiers, x, y };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine(name, [parseResult.GetValue(selector) ?? string.Empty], CompactOptions([
                StringOption("modifiers", parseResult.GetValue(modifiers)),
                IntOption("x", parseResult.GetValue(x)),
                IntOption("y", parseResult.GetValue(y))
            ]))));
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
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
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
        var valueArgument = new Argument<string?>("value")
        {
            Description = "Value to select.",
            Arity = ArgumentArity.ZeroOrOne
        };
        var label = CliStringOption("--label", "Visible option label to select.");
        var optionValue = CliStringOption("--value", "Option value to select.");
        var index = CliIntOption("--index", "Zero-based option index to select.");

        var command = new Command(name, description)
        {
            selectorArgument,
            valueArgument,
            label,
            optionValue,
            index
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
                name,
                CompactArguments(parseResult.GetValue(selectorArgument), parseResult.GetValue(valueArgument)),
                CompactOptions([
                    StringOption("optionLabel", parseResult.GetValue(label)),
                    StringOption("optionValue", parseResult.GetValue(optionValue)),
                    IntOption("index", parseResult.GetValue(index))
                ]))));

        return command;
    }

    private static IReadOnlyList<string> CompactArguments(params string?[] values) =>
        values.Where(value => !string.IsNullOrEmpty(value)).Cast<string>().ToArray();
}

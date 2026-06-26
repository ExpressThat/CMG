using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildPressCommand(BrowserSelectionOptions browserOptions)
    {
        var keyArgument = new Argument<string>("key")
        {
            Description = "Key name or chord to press, such as Enter or Control+A."
        };
        var delay = CliIntOption("--delay", "Delay in milliseconds between keydown and keyup.");

        var command = new Command("press", "Press a keyboard key or shortcut chord.")
        {
            keyArgument,
            delay
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                ToScriptLine(
                    "press",
                    [parseResult.GetValue(keyArgument) ?? string.Empty],
                    CompactOptions([IntOption("delay", parseResult.GetValue(delay))]))));

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
            browserControlCommandHandler.RunScriptAction(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                ToScriptLine(name, parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildKeyboardShortcutCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var chord = new Argument<string>("chord")
        {
            Description = "Keyboard chord such as Control+S or Control+Shift+P."
        };

        var command = new Command(name, "Press a keyboard shortcut chord.")
        {
            chord
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                ToScriptLine("keyboardShortcut", parseResult.GetValue(chord) ?? string.Empty)));

        return command;
    }
}

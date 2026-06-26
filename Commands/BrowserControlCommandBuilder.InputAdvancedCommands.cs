using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildMouseGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("mouse", "Low-level mouse movement and button commands.");
        command.Subcommands.Add(BuildMouseCommand(browserOptions, "move", "mouseMove", "Move the mouse."));
        command.Subcommands.Add(BuildMouseCommand(browserOptions, "down", "mouseDown", "Press the mouse button."));
        command.Subcommands.Add(BuildMouseCommand(browserOptions, "up", "mouseUp", "Release the mouse button."));
        return command;
    }

    private Command BuildMouseCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var target = OptionalTextArgument("target", "Alias target such as center or bottom.");
        var command = new Command(name, description) { target };
        AddPointerOptions(command, out var x, out var y, out var selector, out var edge, out var inset);

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, OptionalArgument(parseResult, target), PointerOptions(parseResult, x, y, selector, edge, inset))));

        return command;
    }

    private Command BuildScrollGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("scroll", "Window, element, and wheel scrolling commands.");
        command.Subcommands.Add(BuildScrollToCommand(browserOptions));
        command.Subcommands.Add(BuildScrollByCommand(browserOptions));
        command.Subcommands.Add(BuildWheelCommand(browserOptions));
        return command;
    }

    private Command BuildScrollToCommand(BrowserSelectionOptions browserOptions)
    {
        var target = OptionalTextArgument("target", "Alias target: top, bottom, left, or right.");
        var command = new Command("to", "Scroll to an absolute position or alias.") { target };
        AddScrollOptions(command, out var x, out var y, out var selector);

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("scrollTo", OptionalArgument(parseResult, target), ScrollOptions(parseResult, x, y, selector))));

        return command;
    }

    private Command BuildScrollByCommand(BrowserSelectionOptions browserOptions)
    {
        var x = new Argument<int>("x") { Description = "Horizontal delta." };
        var y = new Argument<int>("y") { Description = "Vertical delta." };
        var selector = CliStringOption("--selector", "Optional element selector to scroll.");
        var command = new Command("by", "Scroll by a delta.") { x, y, selector };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("scrollBy", [parseResult.GetValue(x).ToString(), parseResult.GetValue(y).ToString()], CompactOptions([
                StringOption("selector", parseResult.GetValue(selector))
            ]))));

        return command;
    }

    private Command BuildWheelCommand(BrowserSelectionOptions browserOptions)
    {
        var target = OptionalTextArgument("target", "Alias or selector target.");
        var command = new Command("wheel", "Dispatch a wheel event and scroll.") { target };
        AddPointerOptions(command, out var x, out var y, out var selector, out var edge, out var inset);
        var deltaX = new Option<int?>("--delta-x") { Description = "Horizontal wheel delta." };
        var deltaY = new Option<int?>("--delta-y") { Description = "Vertical wheel delta. Default is 100." };
        command.Options.Add(deltaX);
        command.Options.Add(deltaY);

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("wheel", OptionalArgument(parseResult, target), CompactOptions([
                .. PointerOptions(parseResult, x, y, selector, edge, inset).Select<(string Key, string Value), (string Key, string Value)?>(option => option),
                IntOption("deltaX", parseResult.GetValue(deltaX)),
                IntOption("deltaY", parseResult.GetValue(deltaY))
            ]))));

        return command;
    }

    private Command BuildClipboardGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("clipboard", "Page-side clipboard shim commands.");
        command.Subcommands.Add(BuildClipboardSetCommand(browserOptions));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "read", "Read clipboard shim text.", "readClipboard"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clear", "Clear clipboard shim text.", "clearClipboard"));
        return command;
    }

    private Command BuildClipboardSetCommand(BrowserSelectionOptions browserOptions)
    {
        var text = new Argument<string>("text") { Description = "Clipboard text." };
        var command = new Command("set", "Set clipboard shim text.") { text };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("setClipboard", parseResult.GetValue(text) ?? string.Empty)));
        return command;
    }

    private Command BuildDispatchEventCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var eventName = new Argument<string>("event") { Description = "DOM event name." };
        var detail = CliStringOption("--detail", "JSON detail payload for CustomEvent.");
        var bubbles = CliStringOption("--bubbles", "Whether the event bubbles: true or false. Default is true.");
        var cancelable = CliStringOption("--cancelable", "Whether the event is cancelable: true or false. Default is true.");
        var command = new Command("dispatchEvent", "Dispatch an Event or CustomEvent on an element.") { selector, eventName, detail, bubbles, cancelable };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("dispatchEvent", [parseResult.GetValue(selector) ?? string.Empty, parseResult.GetValue(eventName) ?? string.Empty], CompactOptions([
                StringOption("detail", parseResult.GetValue(detail)),
                StringOption("bubbles", parseResult.GetValue(bubbles)),
                StringOption("cancelable", parseResult.GetValue(cancelable))
            ]))));

        return command;
    }

    private Command BuildUploadFilesCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var files = new Argument<FileInfo[]>("files")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "One or more local files to assign."
        };
        var command = new Command("uploadFiles", "Assign files to an input[type=file] element.") { selector, files };

        command.SetAction(parseResult =>
        {
            var values = new List<string> { parseResult.GetValue(selector) ?? string.Empty };
            values.AddRange((parseResult.GetValue(files) ?? []).Select(file => file.FullName));
            return browserControlCommandHandler.RunScriptAction(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                ToScriptLine("uploadFiles", values, []));
        });

        return command;
    }

    private static void AddPointerOptions(Command command, out Option<int?> x, out Option<int?> y, out Option<string?> selector, out Option<string?> edge, out Option<int?> inset)
    {
        x = new Option<int?>("--x") { Description = "Viewport x coordinate." };
        y = new Option<int?>("--y") { Description = "Viewport y coordinate." };
        selector = CliStringOption("--selector", "Element selector for edge targeting.");
        edge = CliStringOption("--edge", "Element edge target.");
        inset = new Option<int?>("--inset") { Description = "Inset from element edge." };
        foreach (var option in new Option[] { x, y, selector, edge, inset })
        {
            command.Options.Add(option);
        }
    }

    private static void AddScrollOptions(Command command, out Option<int?> x, out Option<int?> y, out Option<string?> selector)
    {
        x = new Option<int?>("--x") { Description = "Horizontal position." };
        y = new Option<int?>("--y") { Description = "Vertical position." };
        selector = CliStringOption("--selector", "Optional element selector to scroll.");
        command.Options.Add(x);
        command.Options.Add(y);
        command.Options.Add(selector);
    }

    private static Argument<string?> OptionalTextArgument(string name, string description) =>
        new(name) { Arity = ArgumentArity.ZeroOrOne, Description = description };

    private static IReadOnlyList<string> OptionalArgument(ParseResult parseResult, Argument<string?> argument) =>
        string.IsNullOrWhiteSpace(parseResult.GetValue(argument)) ? [] : [parseResult.GetValue(argument)!];

    private static IReadOnlyList<(string Key, string Value)> PointerOptions(
        ParseResult parseResult,
        Option<int?> x,
        Option<int?> y,
        Option<string?> selector,
        Option<string?> edge,
        Option<int?> inset) =>
        CompactOptions([
            IntOption("x", parseResult.GetValue(x)),
            IntOption("y", parseResult.GetValue(y)),
            StringOption("selector", parseResult.GetValue(selector)),
            StringOption("edge", parseResult.GetValue(edge)),
            IntOption("inset", parseResult.GetValue(inset))
        ]);

    private static IReadOnlyList<(string Key, string Value)> ScrollOptions(ParseResult parseResult, Option<int?> x, Option<int?> y, Option<string?> selector) =>
        CompactOptions([
            IntOption("x", parseResult.GetValue(x)),
            IntOption("y", parseResult.GetValue(y)),
            StringOption("selector", parseResult.GetValue(selector))
        ]);
}

using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildFramesGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("frames", "Same-origin iframe interaction commands.");

        command.Subcommands.Add(BuildFrameElementCommand(browserOptions, "click", "frameClick", "Click an element inside an iframe."));
        command.Subcommands.Add(BuildFrameElementCommand(browserOptions, "frameClick", "frameClick", "Click an element inside an iframe."));
        command.Subcommands.Add(BuildFrameElementCommand(browserOptions, "hover", "frameHover", "Hover an element inside an iframe."));
        command.Subcommands.Add(BuildFrameElementCommand(browserOptions, "frameHover", "frameHover", "Hover an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "type", "frameType", "Type text into an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "frameType", "frameType", "Type text into an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "fill", "frameFill", "Fill an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "frameFill", "frameFill", "Fill an element inside an iframe."));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "assertText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "frameAssertText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "expectText", "frameExpectText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "frameExpectText", "frameExpectText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "toHaveText", "frameToHaveText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "frameToHaveText", "frameToHaveText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "toContainText", "frameToContainText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "frameToContainText", "frameToContainText"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "contains", "frameContains"));
        command.Subcommands.Add(BuildFrameAssertTextCommand(browserOptions, "frameContains", "frameContains"));
        command.Subcommands.Add(BuildFrameWaitCommand(browserOptions, "waitForElement"));
        command.Subcommands.Add(BuildFrameWaitCommand(browserOptions, "frameWaitForElement"));
        command.Subcommands.Add(BuildFrameWaitCommand(browserOptions, "waitForSelector", "frameWaitForSelector"));
        command.Subcommands.Add(BuildFrameWaitCommand(browserOptions, "frameWaitForSelector", "frameWaitForSelector"));
        command.Subcommands.Add(BuildFrameEvaluateCommand(browserOptions, "evaluate"));
        command.Subcommands.Add(BuildFrameEvaluateCommand(browserOptions, "frameEvaluate"));
        AddFrameGetterCommands(command, browserOptions);

        return command;
    }

    private Command BuildFrameElementCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var command = new Command(name, description) { frame, selector };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(frame) ?? string.Empty, parseResult.GetValue(selector) ?? string.Empty], [])));
        return command;
    }

    private Command BuildFrameTextCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var text = new Argument<string>("text") { Description = "Text value." };
        var command = new Command(name, description) { frame, selector, text };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(frame) ?? string.Empty,
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(text) ?? string.Empty
            ], [])));
        return command;
    }

    private Command BuildFrameAssertTextCommand(BrowserSelectionOptions browserOptions, string name)
    {
        return BuildFrameAssertTextCommand(browserOptions, name, "frameAssertText");
    }

    private Command BuildFrameAssertTextCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var text = new Argument<string>("text") { Description = "Text value." };
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command(name, "Assert text inside an iframe element.") { frame, selector, text, match, ignoreCase };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(frame) ?? string.Empty,
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(text) ?? string.Empty
            ], CompactOptions([
                StringOption("match", parseResult.GetValue(match)),
                parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
            ]))));
        return command;
    }

    private Command BuildFrameWaitCommand(BrowserSelectionOptions browserOptions, string name)
    {
        return BuildFrameWaitCommand(browserOptions, name, "frameWaitForElement");
    }

    private Command BuildFrameWaitCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds. Default is 5000." };
        var command = new Command(name, "Wait for an element inside an iframe.") { frame, selector, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(frame) ?? string.Empty, parseResult.GetValue(selector) ?? string.Empty], CompactOptions([
                IntOption("timeout", parseResult.GetValue(timeout))
            ]))));
        return command;
    }

    private Command BuildFrameEvaluateCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var frame = FrameArgument();
        var expression = new Argument<string>("expression") { Description = "JavaScript expression evaluated in the iframe." };
        var command = new Command(name, "Evaluate JavaScript inside an iframe.") { frame, expression };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            ToScriptLine("frameEvaluate", [parseResult.GetValue(frame) ?? string.Empty, parseResult.GetValue(expression) ?? string.Empty], [])));
        return command;
    }

    private static Argument<string> FrameArgument() =>
        new("frameSelector") { Description = "CSS selector for the same-origin iframe." };
}

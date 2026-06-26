using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildFramesGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("frames", "Same-origin iframe interaction commands.");

        command.Subcommands.Add(BuildFrameElementCommand(browserOptions, "click", "frameClick", "Click an element inside an iframe."));
        command.Subcommands.Add(BuildFrameElementCommand(browserOptions, "hover", "frameHover", "Hover an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "type", "frameType", "Type text into an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "fill", "frameFill", "Fill an element inside an iframe."));
        command.Subcommands.Add(BuildFrameTextCommand(browserOptions, "assertText", "frameAssertText", "Assert text inside an iframe element."));
        command.Subcommands.Add(BuildFrameWaitCommand(browserOptions));
        command.Subcommands.Add(BuildFrameEvaluateCommand(browserOptions));

        return command;
    }

    private Command BuildFrameElementCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var command = new Command(name, description) { frame, selector };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
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
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(frame) ?? string.Empty,
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(text) ?? string.Empty
            ], [])));
        return command;
    }

    private Command BuildFrameWaitCommand(BrowserSelectionOptions browserOptions)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds. Default is 5000." };
        var command = new Command("waitForElement", "Wait for an element inside an iframe.") { frame, selector, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("frameWaitForElement", [parseResult.GetValue(frame) ?? string.Empty, parseResult.GetValue(selector) ?? string.Empty], CompactOptions([
                IntOption("timeout", parseResult.GetValue(timeout))
            ]))));
        return command;
    }

    private Command BuildFrameEvaluateCommand(BrowserSelectionOptions browserOptions)
    {
        var frame = FrameArgument();
        var expression = new Argument<string>("expression") { Description = "JavaScript expression evaluated in the iframe." };
        var command = new Command("evaluate", "Evaluate JavaScript inside an iframe.") { frame, expression };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("frameEvaluate", [parseResult.GetValue(frame) ?? string.Empty, parseResult.GetValue(expression) ?? string.Empty], [])));
        return command;
    }

    private static Argument<string> FrameArgument() =>
        new("frameSelector") { Description = "CSS selector for the same-origin iframe." };
}

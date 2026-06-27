using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildDragAndDropCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var sourceArgument = new Argument<string>("sourceSelector")
        {
            Description = "CSS selector for the drag source."
        };
        var targetArgument = new Argument<string>("targetSelector")
        {
            Description = "CSS selector for the drop target."
        };
        var sourceX = CliIntOption("--source-x", "X offset inside the source element.");
        var sourceY = CliIntOption("--source-y", "Y offset inside the source element.");
        var targetX = CliIntOption("--target-x", "X offset inside the target element.");
        var targetY = CliIntOption("--target-y", "Y offset inside the target element.");

        var command = new Command(name, "Drag one element onto another.")
        {
            sourceArgument,
            targetArgument,
            sourceX,
            sourceY,
            targetX,
            targetY
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
                name,
                [parseResult.GetValue(sourceArgument) ?? string.Empty, parseResult.GetValue(targetArgument) ?? string.Empty],
                CompactOptions([
                    IntOption("sourceX", parseResult.GetValue(sourceX)),
                    IntOption("sourceY", parseResult.GetValue(sourceY)),
                    IntOption("targetX", parseResult.GetValue(targetX)),
                    IntOption("targetY", parseResult.GetValue(targetY))
                ]))));

        return command;
    }
}

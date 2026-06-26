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

        var command = new Command(name, "Drag one element onto another.")
        {
            sourceArgument,
            targetArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                parseResult.GetValue(sourceArgument) ?? string.Empty,
                parseResult.GetValue(targetArgument) ?? string.Empty)));

        return command;
    }
}

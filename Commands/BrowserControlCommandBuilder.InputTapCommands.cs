using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildTapCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var selector = new Argument<string?>("selector")
        {
            Description = "CSS selector or CMG rich locator to tap.",
            Arity = ArgumentArity.ZeroOrOne
        };
        var x = CliIntOption("--x", "Viewport X coordinate to tap.");
        var y = CliIntOption("--y", "Viewport Y coordinate to tap.");

        var command = new Command(name, "Tap an element or viewport coordinate with touch-style events.")
        {
            selector,
            x,
            y
        };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, CompactArguments(parseResult.GetValue(selector)), CompactOptions([
                IntOption("x", parseResult.GetValue(x)),
                IntOption("y", parseResult.GetValue(y))
            ]))));

        return command;
    }
}

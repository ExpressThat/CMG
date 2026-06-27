using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildNavigateCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var target = new Argument<string>("target")
        {
            Description = "URL, data URL, or local file path."
        };
        var waitUntil = new Option<string?>("--wait-until")
        {
            Description = "State to wait for after navigation: load, domcontentloaded, networkidle, or commit."
        };
        var timeout = new Option<int?>("--timeout")
        {
            Description = "Timeout in milliseconds when --wait-until is provided."
        };
        var command = new Command(name, "Navigate the primary page target.")
        {
            target,
            waitUntil,
            timeout
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
                name,
                [parseResult.GetValue(target) ?? string.Empty],
                CompactOptions([
                    StringOption("waitUntil", parseResult.GetValue(waitUntil)),
                    IntOption("timeout", parseResult.GetValue(timeout))
                ]))));

        return command;
    }
}

using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildReloadCommand(BrowserSelectionOptions browserOptions)
    {
        var waitUntil = new Option<string?>("--wait-until")
        {
            Description = "State to wait for after reload: load, domcontentloaded, networkidle, or commit."
        };
        var timeout = new Option<int?>("--timeout")
        {
            Description = "Timeout in milliseconds when --wait-until is provided."
        };
        var command = new Command("reload", "Reload the primary page target.")
        {
            waitUntil,
            timeout
        };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("reload", [], CompactOptions([
                StringOption("waitUntil", parseResult.GetValue(waitUntil)),
                IntOption("timeout", parseResult.GetValue(timeout))
            ]))));

        return command;
    }
}

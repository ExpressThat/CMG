using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildHistoryCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var timeout = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };
        var waitUntil = new Option<string?>("--wait-until")
        {
            Description = "State to wait for after history navigation: load, domcontentloaded, networkidle, or commit."
        };
        var command = new Command(name, description)
        {
            timeout,
            waitUntil
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                [],
                CompactOptions([
                    ("timeout", $"{parseResult.GetValue(timeout)}"),
                    StringOption("waitUntil", parseResult.GetValue(waitUntil))
                ]))));

        return command;
    }
}

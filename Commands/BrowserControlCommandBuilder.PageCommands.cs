using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildPageGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("page", "Page evaluation, viewport, and utility commands.");

        command.Subcommands.Add(BuildEvaluateCommand(browserOptions));
        command.Subcommands.Add(BuildSetViewportCommand(browserOptions));
        command.Subcommands.Add(BuildShowMessageBarCommand(browserOptions));
        command.Subcommands.Add(BuildDelayCommand(browserOptions));
        command.Subcommands.Add(BuildPageRuntimeGroup(browserOptions));

        return command;
    }

    private Command BuildAssertionsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("assertions", "Page and element assertion commands.");

        command.Subcommands.Add(BuildAssertTextCommand(browserOptions));

        return command;
    }
}

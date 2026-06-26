using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private readonly IBrowserControlCommandHandler browserControlCommandHandler;

    public BrowserControlCommandBuilder(IBrowserControlCommandHandler browserControlCommandHandler)
    {
        this.browserControlCommandHandler = browserControlCommandHandler;
    }

    public Command Build(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("control", "Browser interaction and page control commands.");

        command.SetAction(_ =>
        {
            Console.Error.WriteLine("No browser control command was provided.");
            Console.Error.WriteLine("Run 'cmg browser control --help' to see available commands.");

            return 1;
        });

        command.Subcommands.Add(BuildScriptCommand(browserOptions));
        command.Subcommands.Add(BuildValidateScriptCommand());
        command.Subcommands.Add(BuildNavigationGroup(browserOptions));
        command.Subcommands.Add(BuildInputGroup(browserOptions));
        command.Subcommands.Add(BuildTabsGroup(browserOptions));
        command.Subcommands.Add(BuildCaptureGroup(browserOptions));
        command.Subcommands.Add(BuildPageGroup(browserOptions));
        command.Subcommands.Add(BuildAssertionsGroup(browserOptions));
        command.Subcommands.Add(BuildStorageGroup(browserOptions));
        command.Subcommands.Add(BuildNetworkGroup(browserOptions));
        command.Subcommands.Add(BuildEventsGroup(browserOptions));
        command.Subcommands.Add(BuildContextGroup(browserOptions));

        return command;
    }
}

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

        command.Subcommands.Add(BuildGetElementCommand(browserOptions));
        command.Subcommands.Add(BuildScriptCommand(browserOptions));
        command.Subcommands.Add(BuildNavigateCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForElementCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "click", "Click an element."));
        command.Subcommands.Add(BuildTypeCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "clear", "Clear an input-like element."));
        command.Subcommands.Add(BuildPressCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "hover", "Hover an element."));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "scrollIntoView", "Scroll an element into view."));
        command.Subcommands.Add(BuildSelectCommand(browserOptions));
        command.Subcommands.Add(BuildShowMessageBarCommand(browserOptions));
        command.Subcommands.Add(BuildDelayCommand(browserOptions));
        command.Subcommands.Add(BuildSelectorCommand(browserOptions, "html", "Print an element's outer HTML."));
        command.Subcommands.Add(BuildScreenshotCommand(browserOptions));
        command.Subcommands.Add(BuildScreenshotPageCommand(browserOptions));
        command.Subcommands.Add(BuildAssertTextCommand(browserOptions));
        command.Subcommands.Add(BuildEvaluateCommand(browserOptions));
        command.Subcommands.Add(BuildSetViewportCommand(browserOptions));
        command.Subcommands.Add(BuildDragAndDropCommand(browserOptions));
        command.Subcommands.Add(BuildNoArgumentCommand(browserOptions, "listTabs", "List available page targets."));
        command.Subcommands.Add(BuildIndexedCommand(browserOptions, "activateTab", "Activate a page target by index."));
        command.Subcommands.Add(BuildIndexedCommand(browserOptions, "closeTab", "Close a page target by index."));
        command.Subcommands.Add(BuildSetCommand(browserOptions));

        return command;
    }
}

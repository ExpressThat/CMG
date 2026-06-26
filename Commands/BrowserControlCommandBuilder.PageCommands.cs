using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildPageGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("page", "Page evaluation, viewport, and utility commands.");

        command.Subcommands.Add(BuildEvaluateCommand(browserOptions));
        command.Subcommands.Add(BuildSetViewportCommand(browserOptions, "setViewport"));
        command.Subcommands.Add(BuildSetViewportCommand(browserOptions, "viewport"));
        command.Subcommands.Add(BuildSetViewportCommand(browserOptions, "setViewportSize"));
        command.Subcommands.Add(BuildShowMessageBarCommand(browserOptions));
        command.Subcommands.Add(BuildDelayCommand(browserOptions));
        command.Subcommands.Add(BuildPageRuntimeGroup(browserOptions));

        return command;
    }

    private Command BuildAssertionsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("assertions", "Page and element assertion commands.");

        command.Subcommands.Add(BuildAssertTextCommand(browserOptions));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "expectText", "Assert that an element contains text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "toHaveText", "Assert that an element contains text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "toContainText", "Assert that an element contains text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "containsText", "Assert that an element contains text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "waitForText", "Wait until an element contains text."));
        command.Subcommands.Add(BuildBodyContainsCommand(browserOptions));
        command.Subcommands.Add(BuildEvaluateAssertionCommand(browserOptions));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "visible", "expectVisible", "Assert that an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "hidden", "expectHidden", "Assert that an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "enabled", "expectEnabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "disabled", "expectDisabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions));

        return command;
    }
}

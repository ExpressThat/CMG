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
        command.Subcommands.Add(BuildShowMessageBarCommand(browserOptions, "showMessageBar"));
        command.Subcommands.Add(BuildShowMessageBarCommand(browserOptions, "caption"));
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
        command.Subcommands.Add(BuildEvaluateAssertionCommand(browserOptions, "eval", "expectEval", "Assert a JavaScript expression result."));
        command.Subcommands.Add(BuildEvaluateAssertionCommand(browserOptions, "expectEval", "expectEval", "Assert a JavaScript expression result."));
        command.Subcommands.Add(BuildEvaluateAssertionCommand(browserOptions, "assertEval", "assertEval", "Assert a JavaScript expression result."));
        command.Subcommands.Add(BuildEvaluateAssertionCommand(browserOptions, "expectExpression", "expectExpression", "Assert a JavaScript expression result."));
        command.Subcommands.Add(BuildEvaluateAssertionCommand(browserOptions, "assertExpression", "assertExpression", "Assert a JavaScript expression result."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "assertVisible", "assertVisible", "Assert that an element exists and is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "visible", "expectVisible", "Assert that an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "hidden", "expectHidden", "Assert that an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "enabled", "expectEnabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "disabled", "expectDisabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectVisible", "expectVisible", "Assert that an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeVisible", "toBeVisible", "Assert that an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "waitForVisible", "waitForVisible", "Wait until an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectHidden", "expectHidden", "Assert that an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeHidden", "toBeHidden", "Assert that an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "waitForHidden", "waitForHidden", "Wait until an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectEnabled", "expectEnabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeEnabled", "toBeEnabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectDisabled", "expectDisabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeDisabled", "toBeDisabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectAttached", "expectAttached", "Assert that an element is attached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeAttached", "toBeAttached", "Assert that an element is attached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectDetached", "expectDetached", "Assert that an element is detached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeDetached", "toBeDetached", "Assert that an element is detached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectEditable", "expectEditable", "Assert that an element is editable."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeEditable", "toBeEditable", "Assert that an element is editable."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectEmpty", "expectEmpty", "Assert that an element is empty."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeEmpty", "toBeEmpty", "Assert that an element is empty."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectFocused", "expectFocused", "Assert that an element is focused."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeFocused", "toBeFocused", "Assert that an element is focused."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectInViewport", "expectInViewport", "Assert that an element intersects the viewport."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeInViewport", "toBeInViewport", "Assert that an element intersects the viewport."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "value", "expectValue"));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "expectValue", "expectValue"));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "toHaveValue", "toHaveValue"));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "attribute", "expectAttribute"));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "expectAttribute", "expectAttribute"));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "toHaveAttribute", "toHaveAttribute"));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions, "checked", "expectChecked"));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions, "expectChecked", "expectChecked"));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions, "toBeChecked", "toBeChecked"));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions, "count", "expectCount"));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions, "expectCount", "expectCount"));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions, "toHaveCount", "toHaveCount"));

        return command;
    }
}

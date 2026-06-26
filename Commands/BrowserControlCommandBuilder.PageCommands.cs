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
        command.Subcommands.Add(BuildHighlightCommand(browserOptions));
        command.Subcommands.Add(BuildDelayCommand(browserOptions));
        command.Subcommands.Add(BuildPageRuntimeGroup(browserOptions));

        return command;
    }

    private Command BuildHighlightCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var color = CliStringOption("--color", "Highlight border and message tag color. Default is #f59e0b.");
        var message = CliStringOption("--message", "Optional message shown above the highlighted element.");
        var duration = CliIntOption("--duration", "Highlight duration in milliseconds. Default is 1200.");
        var command = new Command("highlight", "Draw a temporary visual highlight around an element.")
        {
            selector, color, message, duration
        };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("highlight", [parseResult.GetValue(selector) ?? string.Empty], CompactOptions([
                StringOption("color", parseResult.GetValue(color)),
                StringOption("message", parseResult.GetValue(message)),
                IntOption("duration", parseResult.GetValue(duration))
            ]))));

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
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "expectNoText", "Assert that an element does not contain text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "expectNotText", "Assert that an element does not contain text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "notContainsText", "Assert that an element does not contain text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "toNotContainText", "Assert that an element does not contain text."));
        command.Subcommands.Add(BuildTextAssertionCommand(browserOptions, "toHaveNoText", "Assert that an element does not contain text."));
        command.Subcommands.Add(BuildBodyTextAssertionCommand(browserOptions, "contains", "Assert that the page body contains text."));
        command.Subcommands.Add(BuildBodyTextAssertionCommand(browserOptions, "notContains", "Assert that the page body does not contain text."));
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
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotVisible", "expectNotVisible", "Assert that an element is not visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotVisible", "toBeNotVisible", "Assert that an element is not visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "waitForVisible", "waitForVisible", "Wait until an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectHidden", "expectHidden", "Assert that an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeHidden", "toBeHidden", "Assert that an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotHidden", "expectNotHidden", "Assert that an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotHidden", "toBeNotHidden", "Assert that an element is visible."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "waitForHidden", "waitForHidden", "Wait until an element is hidden."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectEnabled", "expectEnabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeEnabled", "toBeEnabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotEnabled", "expectNotEnabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotEnabled", "toBeNotEnabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectDisabled", "expectDisabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeDisabled", "toBeDisabled", "Assert that an element is disabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotDisabled", "expectNotDisabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotDisabled", "toBeNotDisabled", "Assert that an element is enabled."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectAttached", "expectAttached", "Assert that an element is attached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeAttached", "toBeAttached", "Assert that an element is attached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotAttached", "expectNotAttached", "Assert that an element is detached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotAttached", "toBeNotAttached", "Assert that an element is detached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectDetached", "expectDetached", "Assert that an element is detached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeDetached", "toBeDetached", "Assert that an element is detached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotDetached", "expectNotDetached", "Assert that an element is attached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotDetached", "toBeNotDetached", "Assert that an element is attached."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectEditable", "expectEditable", "Assert that an element is editable."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeEditable", "toBeEditable", "Assert that an element is editable."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotEditable", "expectNotEditable", "Assert that an element is not editable."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotEditable", "toBeNotEditable", "Assert that an element is not editable."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectEmpty", "expectEmpty", "Assert that an element is empty."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeEmpty", "toBeEmpty", "Assert that an element is empty."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotEmpty", "expectNotEmpty", "Assert that an element is not empty."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotEmpty", "toBeNotEmpty", "Assert that an element is not empty."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectFocused", "expectFocused", "Assert that an element is focused."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeFocused", "toBeFocused", "Assert that an element is focused."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotFocused", "expectNotFocused", "Assert that an element is not focused."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotFocused", "toBeNotFocused", "Assert that an element is not focused."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectInViewport", "expectInViewport", "Assert that an element intersects the viewport."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeInViewport", "toBeInViewport", "Assert that an element intersects the viewport."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotInViewport", "expectNotInViewport", "Assert that an element does not intersect the viewport."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotInViewport", "toBeNotInViewport", "Assert that an element does not intersect the viewport."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "value", "expectValue"));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "expectValue", "expectValue"));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "toHaveValue", "toHaveValue"));
        command.Subcommands.Add(BuildElementValuesAssertionCommand(browserOptions, "expectValues", "expectValues"));
        command.Subcommands.Add(BuildElementValuesAssertionCommand(browserOptions, "toHaveValues", "toHaveValues"));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "attribute", "expectAttribute"));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "expectAttribute", "expectAttribute"));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "toHaveAttribute", "toHaveAttribute"));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "expectClass", "expectClass", "Assert that an element class contains text.", "Expected class token or fragment."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "toHaveClass", "toHaveClass", "Assert that an element class contains text.", "Expected class token or fragment."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "expectId", "expectId", "Assert that an element id matches text.", "Expected id."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "toHaveId", "toHaveId", "Assert that an element id matches text.", "Expected id."));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "expectCSS", "expectCSS", "Assert that a computed CSS property contains text.", "CSS property name.", "Expected CSS value fragment."));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "toHaveCSS", "toHaveCSS", "Assert that a computed CSS property contains text.", "CSS property name.", "Expected CSS value fragment."));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "expectProperty", "expectProperty", "Assert that a DOM property contains text.", "DOM property path.", "Expected property value fragment."));
        command.Subcommands.Add(BuildElementAttributeAssertionCommand(browserOptions, "toHaveJSProperty", "toHaveJSProperty", "Assert that a DOM property contains text.", "DOM property path.", "Expected property value fragment."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "expectAccessibleName", "expectAccessibleName", "Assert that an element accessible name contains text.", "Expected accessible name fragment."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "toHaveAccessibleName", "toHaveAccessibleName", "Assert that an element accessible name contains text.", "Expected accessible name fragment."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "expectRole", "expectRole", "Assert that an element role matches text.", "Expected role."));
        command.Subcommands.Add(BuildElementValueAssertionCommand(browserOptions, "toHaveRole", "toHaveRole", "Assert that an element role matches text.", "Expected role."));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions, "checked", "expectChecked"));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions, "expectChecked", "expectChecked"));
        command.Subcommands.Add(BuildElementCheckedAssertionCommand(browserOptions, "toBeChecked", "toBeChecked"));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectNotChecked", "expectNotChecked", "Assert that an element is unchecked."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeNotChecked", "toBeNotChecked", "Assert that an element is unchecked."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "unchecked", "expectUnchecked", "Assert that an element is unchecked."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "expectUnchecked", "expectUnchecked", "Assert that an element is unchecked."));
        command.Subcommands.Add(BuildElementStateAssertionCommand(browserOptions, "toBeUnchecked", "toBeUnchecked", "Assert that an element is unchecked."));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions, "count", "expectCount"));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions, "expectCount", "expectCount"));
        command.Subcommands.Add(BuildElementCountAssertionCommand(browserOptions, "toHaveCount", "toHaveCount"));

        return command;
    }
}

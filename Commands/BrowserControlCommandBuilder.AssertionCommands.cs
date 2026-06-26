using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildEvaluateAssertionCommand(BrowserSelectionOptions browserOptions)
    {
        var expression = new Argument<string>("expression") { Description = "JavaScript expression to evaluate." };
        var equals = new Option<string?>("--equals") { Description = "Expected exact string value." };
        var contains = new Option<string?>("--contains") { Description = "Expected substring." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command("eval", "Assert a JavaScript expression result.") { expression, equals, contains, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("expectEval", [parseResult.GetValue(expression) ?? string.Empty], CompactOptions([
                StringOption("equals", parseResult.GetValue(equals)),
                StringOption("contains", parseResult.GetValue(contains)),
                IntOption("timeout", parseResult.GetValue(timeout))
            ]))));
        return command;
    }

    private Command BuildElementStateAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action,
        string description)
    {
        var selector = CreateSelectorArgument();
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, description) { selector, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(selector) ?? string.Empty], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementValueAssertionCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var expected = new Argument<string>("expected") { Description = "Expected value fragment." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command("value", "Assert that an element value contains text.") { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("expectValue", [
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(expected) ?? string.Empty
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementAttributeAssertionCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var name = new Argument<string>("name") { Description = "Attribute name." };
        var expected = new Argument<string>("expected") { Description = "Expected attribute value fragment." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command("attribute", "Assert that an element attribute contains text.") { selector, name, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("expectAttribute", [
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(name) ?? string.Empty,
                parseResult.GetValue(expected) ?? string.Empty
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementCheckedAssertionCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var expected = new Option<bool?>("--expected") { Description = "Expected checked state. Defaults to true." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command("checked", "Assert that an element is checked or unchecked.") { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("expectChecked", CheckedArguments(parseResult, selector, expected), TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementCountAssertionCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var expected = new Argument<int>("expected") { Description = "Expected matching element count." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command("count", "Assert the number of matching elements.") { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("expectCount", [
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(expected).ToString()
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private static IReadOnlyList<string> CheckedArguments(ParseResult parseResult, Argument<string> selector, Option<bool?> expected)
    {
        var selectorValue = parseResult.GetValue(selector) ?? string.Empty;
        var expectedValue = parseResult.GetValue(expected);
        return expectedValue is null ? [selectorValue] : [selectorValue, expectedValue.Value.ToString().ToLowerInvariant()];
    }

    private static IReadOnlyList<(string Key, string Value)> TimeoutOptions(ParseResult parseResult, Option<int?> timeout) =>
        CompactOptions([IntOption("timeout", parseResult.GetValue(timeout))]);
}

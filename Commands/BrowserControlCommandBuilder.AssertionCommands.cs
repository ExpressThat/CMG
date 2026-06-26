using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildTextAssertionCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var selector = CreateSelectorArgument();
        var expected = new Argument<string>("expected") { Description = "Expected text fragment." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(action, description) { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(expected) ?? string.Empty
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildBodyTextAssertionCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var expected = new Argument<string>("expected") { Description = "Expected body text fragment." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(action, description) { expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(expected) ?? string.Empty], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildEvaluateAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action,
        string description)
    {
        var expression = new Argument<string>("expression") { Description = "JavaScript expression to evaluate." };
        var equals = new Option<string?>("--equals") { Description = "Expected exact string value." };
        var contains = new Option<string?>("--contains") { Description = "Expected substring." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, description) { expression, equals, contains, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(expression) ?? string.Empty], CompactOptions([
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

    private Command BuildElementValueAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action,
        string description = "Assert that an element value contains text.",
        string expectedDescription = "Expected value fragment.")
    {
        var selector = CreateSelectorArgument();
        var expected = new Argument<string>("expected") { Description = expectedDescription };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, description) { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(expected) ?? string.Empty
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementValuesAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action)
    {
        var selector = CreateSelectorArgument();
        var expected = new Argument<string[]>("expected")
        {
            Description = "Expected selected values in order.",
            Arity = ArgumentArity.OneOrMore
        };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, "Assert that selected values match in order.") { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(selector) ?? string.Empty,
                .. (parseResult.GetValue(expected) ?? [])
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementAttributeAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string commandName,
        string action,
        string description = "Assert that an element attribute contains text.",
        string nameDescription = "Attribute name.",
        string expectedDescription = "Expected attribute value fragment.")
    {
        var selector = CreateSelectorArgument();
        var name = new Argument<string>("name") { Description = nameDescription };
        var expected = new Argument<string>("expected") { Description = expectedDescription };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(commandName, description) { selector, name, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(name) ?? string.Empty,
                parseResult.GetValue(expected) ?? string.Empty
            ], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementCheckedAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action)
    {
        var selector = CreateSelectorArgument();
        var expected = new Option<bool?>("--expected") { Description = "Expected checked state. Defaults to true." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, "Assert that an element is checked or unchecked.") { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, CheckedArguments(parseResult, selector, expected), TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildElementCountAssertionCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action)
    {
        var selector = CreateSelectorArgument();
        var expected = new Argument<int>("expected") { Description = "Expected matching element count." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, "Assert the number of matching elements.") { selector, expected, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
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

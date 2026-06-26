using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildWaitGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("wait", "Page synchronization wait commands.");

        command.Subcommands.Add(BuildWaitSelectorCommand(browserOptions, "element", "waitForElement", "Wait until an element exists."));
        command.Subcommands.Add(BuildWaitSelectorCommand(browserOptions, "selector", "waitForSelector", "Wait until a selector exists."));
        command.Subcommands.Add(BuildWaitFunctionCommand(browserOptions, "function"));
        command.Subcommands.Add(BuildWaitTimeoutCommand(browserOptions, "timeout"));
        command.Subcommands.Add(BuildWaitSelectorCommand(browserOptions, "waitForElement", "waitForElement", "Alias for element."));
        command.Subcommands.Add(BuildWaitSelectorCommand(browserOptions, "waitForSelector", "waitForSelector", "Alias for selector."));
        command.Subcommands.Add(BuildWaitFunctionCommand(browserOptions, "waitForFunction"));
        command.Subcommands.Add(BuildWaitTimeoutCommand(browserOptions, "waitForTimeout"));
        command.Subcommands.Add(BuildWaitAliasCommand(browserOptions));

        return command;
    }

    private Command BuildWaitSelectorCommand(
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

    private Command BuildWaitAliasCommand(BrowserSelectionOptions browserOptions)
    {
        var target = new Argument<string>("target") { Description = "Milliseconds or selector to wait for." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds when target is a selector." };
        var command = new Command("auto", "Wait for milliseconds or for an element selector.") { target, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("wait", [parseResult.GetValue(target) ?? string.Empty], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildWaitFunctionCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var expression = new Argument<string>("expression") { Description = "JavaScript expression that must become truthy." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command(name, "Wait until a JavaScript expression becomes truthy.") { expression, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForFunction", [parseResult.GetValue(expression) ?? string.Empty], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildWaitTimeoutCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var milliseconds = new Argument<int>("milliseconds") { Description = "Delay duration in milliseconds." };
        var command = new Command(name, "Wait for a fixed duration.") { milliseconds };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForTimeout", parseResult.GetValue(milliseconds).ToString())));
        return command;
    }
}

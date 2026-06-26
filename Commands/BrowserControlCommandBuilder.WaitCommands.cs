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
        command.Subcommands.Add(BuildWaitFunctionCommand(browserOptions));
        command.Subcommands.Add(BuildWaitTimeoutCommand(browserOptions));

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

    private Command BuildWaitFunctionCommand(BrowserSelectionOptions browserOptions)
    {
        var expression = new Argument<string>("expression") { Description = "JavaScript expression that must become truthy." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var command = new Command("function", "Wait until a JavaScript expression becomes truthy.") { expression, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForFunction", [parseResult.GetValue(expression) ?? string.Empty], TimeoutOptions(parseResult, timeout))));
        return command;
    }

    private Command BuildWaitTimeoutCommand(BrowserSelectionOptions browserOptions)
    {
        var milliseconds = new Argument<int>("milliseconds") { Description = "Delay duration in milliseconds." };
        var command = new Command("timeout", "Wait for a fixed duration.") { milliseconds };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForTimeout", parseResult.GetValue(milliseconds).ToString())));
        return command;
    }
}

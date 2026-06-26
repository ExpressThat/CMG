using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildNoArgumentScriptCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var command = new Command(name, description);

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), name));

        return command;
    }

    private Command BuildHistoryCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };

        var command = new Command(name, description)
        {
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                [],
                [("timeout", parseResult.GetValue(timeoutOption).ToString())])));

        return command;
    }

    private Command BuildWaitForUrlCommand(BrowserSelectionOptions browserOptions)
    {
        var expectedArgument = new Argument<string>("expected")
        {
            Description = "Expected URL substring."
        };
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };

        var command = new Command("waitForUrl", "Wait until the current URL contains text.")
        {
            expectedArgument,
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "waitForUrl",
                [parseResult.GetValue(expectedArgument) ?? string.Empty],
                [("timeout", parseResult.GetValue(timeoutOption).ToString())])));

        return command;
    }

    private Command BuildWaitForLoadStateCommand(BrowserSelectionOptions browserOptions)
    {
        var stateArgument = new Argument<string>("state")
        {
            Description = "Load state: loading, interactive, complete, load, or networkidle.",
            DefaultValueFactory = _ => "load"
        };
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };

        var command = new Command("waitForLoadState", "Wait until the page reaches a load state.")
        {
            stateArgument,
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "waitForLoadState",
                [parseResult.GetValue(stateArgument) ?? "load"],
                [("timeout", parseResult.GetValue(timeoutOption).ToString())])));

        return command;
    }

    private Command BuildSetContentCommand(BrowserSelectionOptions browserOptions)
    {
        var htmlArgument = new Argument<string>("html")
        {
            Description = "HTML to assign to document.documentElement.innerHTML."
        };

        var command = new Command("setContent", "Replace the current page HTML.")
        {
            htmlArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "setContent",
                parseResult.GetValue(htmlArgument) ?? string.Empty)));

        return command;
    }
}

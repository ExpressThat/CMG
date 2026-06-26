using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildNavigationGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("navigation", "Navigation and page state commands.");

        command.Subcommands.Add(BuildNavigateCommand(browserOptions));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "reload", "Reload the primary page target."));
        command.Subcommands.Add(BuildHistoryCommand(browserOptions, "goBack", "Navigate one step back in page history."));
        command.Subcommands.Add(BuildHistoryCommand(browserOptions, "goForward", "Navigate one step forward in page history."));
        command.Subcommands.Add(BuildWaitForUrlCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForLoadStateCommand(browserOptions));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "url", "Print the current page URL."));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "title", "Print the current page title."));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "content", "Print the current page HTML."));
        command.Subcommands.Add(BuildSetContentCommand(browserOptions));

        return command;
    }

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

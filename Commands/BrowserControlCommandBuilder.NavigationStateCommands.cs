using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildNavigationGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("navigation", "Navigation and page state commands.");

        command.Subcommands.Add(BuildNavigateCommand(browserOptions, "navigate"));
        command.Subcommands.Add(BuildNavigateCommand(browserOptions, "goto"));
        command.Subcommands.Add(BuildNavigateCommand(browserOptions, "visit"));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "reload", "Reload the primary page target."));
        command.Subcommands.Add(BuildHistoryCommand(browserOptions, "goBack", "Navigate one step back in page history."));
        command.Subcommands.Add(BuildHistoryCommand(browserOptions, "goForward", "Navigate one step forward in page history."));
        command.Subcommands.Add(BuildWaitForNavigationValueCommand(browserOptions, "waitForUrl", "Wait until the current URL matches text.", "Expected URL text."));
        command.Subcommands.Add(BuildWaitForNavigationValueCommand(browserOptions, "waitForTitle", "Wait until the current page title matches text.", "Expected title text."));
        command.Subcommands.Add(BuildExpectNavigationValueCommand(browserOptions, "expectUrl", "Assert that the current URL matches text."));
        command.Subcommands.Add(BuildExpectNavigationValueCommand(browserOptions, "expectTitle", "Assert that the current page title matches text."));
        command.Subcommands.Add(BuildExpectNavigationValueCommand(browserOptions, "toHaveURL", "Assert that the current URL matches text."));
        command.Subcommands.Add(BuildExpectNavigationValueCommand(browserOptions, "toHaveTitle", "Assert that the current page title matches text."));
        command.Subcommands.Add(BuildWaitForLoadStateCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForNetworkIdleCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForNavigationCommand(browserOptions));
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
                [("timeout", $"{parseResult.GetValue(timeoutOption)}")])));

        return command;
    }

    private Command BuildWaitForNavigationValueCommand(BrowserSelectionOptions browserOptions, string name, string description, string argumentDescription)
    {
        var expectedArgument = new Argument<string>("expected") { Description = argumentDescription };
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };
        var matchOption = NavigationMatchOption();
        var ignoreCaseOption = NavigationIgnoreCaseOption();

        var command = new Command(name, description)
        {
            expectedArgument,
            timeoutOption,
            matchOption,
            ignoreCaseOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                [parseResult.GetValue(expectedArgument) ?? string.Empty],
                NavigationMatchOptions(parseResult, timeoutOption, matchOption, ignoreCaseOption))));

        return command;
    }

    private Command BuildExpectNavigationValueCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var expectedArgument = new Argument<string>("expected")
        {
            Description = name.Contains("Title", StringComparison.Ordinal) ? "Expected title text." : "Expected URL text."
        };
        var matchOption = NavigationMatchOption();
        var ignoreCaseOption = NavigationIgnoreCaseOption();

        var command = new Command(name, description)
        {
            expectedArgument,
            matchOption,
            ignoreCaseOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                name,
                [parseResult.GetValue(expectedArgument) ?? string.Empty],
                NavigationMatchOptions(parseResult, null, matchOption, ignoreCaseOption))));

        return command;
    }

    private static Option<string?> NavigationMatchOption() =>
        new("--match") { Description = "Match mode: contains, exact, or regex." };

    private static Option<bool> NavigationIgnoreCaseOption() =>
        new("--ignore-case") { Description = "Use case-insensitive matching." };

    private static IReadOnlyList<(string Key, string Value)> NavigationMatchOptions(
        ParseResult parseResult,
        Option<int>? timeout,
        Option<string?> match,
        Option<bool> ignoreCase) =>
        CompactOptions([
            timeout is null ? null : ("timeout", parseResult.GetValue(timeout).ToString()),
            StringOption("match", parseResult.GetValue(match)),
            parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
        ]);

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

    private Command BuildWaitForNetworkIdleCommand(BrowserSelectionOptions browserOptions)
    {
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };

        var command = new Command("waitForNetworkIdle", "Wait until the page reaches CMG network idle.")
        {
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "waitForNetworkIdle",
                [],
                CompactOptions([
                    IntOption("timeout", parseResult.GetValue(timeoutOption))
                ]))));

        return command;
    }

    private Command BuildWaitForNavigationCommand(BrowserSelectionOptions browserOptions)
    {
        var expectedArgument = new Argument<string?>("expected")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Optional URL substring expected after navigation."
        };
        var waitUntilOption = new Option<string?>("--wait-until")
        {
            Description = "Load state: load, domcontentloaded, networkidle, or commit."
        };
        var timeoutOption = new Option<int?>("--timeout")
        {
            Description = "Timeout in milliseconds."
        };

        var command = new Command("waitForNavigation", "Wait until navigation reaches a state.")
        {
            expectedArgument,
            waitUntilOption,
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "waitForNavigation",
                Compact([parseResult.GetValue(expectedArgument)]),
                CompactOptions([
                    StringOption("waitUntil", parseResult.GetValue(waitUntilOption)),
                    IntOption("timeout", parseResult.GetValue(timeoutOption))
                ]))));

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

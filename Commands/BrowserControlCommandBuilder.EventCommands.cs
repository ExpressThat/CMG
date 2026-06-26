using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildEventsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("events", "Downloads, dialogs, console, page-error, and generic event waits.");

        command.Subcommands.Add(BuildDownloadCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForDownloadCommand(browserOptions));
        command.Subcommands.Add(BuildConsoleGroup(browserOptions));
        command.Subcommands.Add(BuildDialogsGroup(browserOptions));
        command.Subcommands.Add(BuildPageErrorsGroup(browserOptions));
        command.Subcommands.Add(BuildWaitForEventCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForEventCommand(browserOptions, "waitForEvent"));

        return command;
    }

    private Command BuildDownloadCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var command = new Command("download", "Click an element and wait for a matching download.") { selector };
        AddDownloadOptions(command, out var directory, out var pattern, out var timeout);

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("download", [parseResult.GetValue(selector) ?? string.Empty], DownloadOptions(parseResult, directory, pattern, timeout))));

        return command;
    }

    private Command BuildWaitForDownloadCommand(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("waitForDownload", "Wait for a matching downloaded file.");
        AddDownloadOptions(command, out var directory, out var pattern, out var timeout);

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForDownload", [], DownloadOptions(parseResult, directory, pattern, timeout))));

        return command;
    }

    private Command BuildConsoleGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("console", "Console capture and wait commands.");
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "capture", "Capture future console messages.", "captureConsole"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "captureConsole", "Capture future console messages.", "captureConsole"));
        command.Subcommands.Add(BuildMessageWaitCommand(browserOptions, "wait", "waitForConsole", "Wait for a matching console message.", includeLevel: true));
        command.Subcommands.Add(BuildMessageWaitCommand(browserOptions, "waitForConsole", "waitForConsole", "Wait for a matching console message.", includeLevel: true));
        command.Subcommands.Add(BuildNoConsoleCommand(browserOptions, "expectNoConsole", "expectNoConsole"));
        command.Subcommands.Add(BuildNoConsoleCommand(browserOptions, "toHaveNoConsole", "toHaveNoConsole"));
        return command;
    }

    private Command BuildDialogsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("dialogs", "Browser dialog capture, behavior, and wait commands.");
        command.Subcommands.Add(BuildDialogCaptureCommand(browserOptions, "capture", "captureDialogs"));
        command.Subcommands.Add(BuildDialogCaptureCommand(browserOptions, "captureDialogs", "captureDialogs"));
        command.Subcommands.Add(BuildDialogBehaviorCommand(browserOptions, "behavior", "setDialogBehavior"));
        command.Subcommands.Add(BuildDialogBehaviorCommand(browserOptions, "setDialogBehavior", "setDialogBehavior"));
        command.Subcommands.Add(BuildDialogBehaviorCommand(browserOptions, "onDialog", "onDialog"));
        command.Subcommands.Add(BuildDialogBehaviorCommand(browserOptions, "handleDialog", "handleDialog"));
        command.Subcommands.Add(BuildDialogBehaviorCommand(browserOptions, "dialogBehavior", "dialogBehavior"));
        command.Subcommands.Add(BuildMessageWaitCommand(browserOptions, "wait", "waitForDialog", "Wait for a matching browser dialog.", includeLevel: false));
        command.Subcommands.Add(BuildMessageWaitCommand(browserOptions, "waitForDialog", "waitForDialog", "Wait for a matching browser dialog.", includeLevel: false));
        return command;
    }

    private Command BuildPageErrorsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("pageErrors", "Page error capture, wait, and absence assertion commands.");
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "capture", "Capture future page errors.", "capturePageErrors"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "capturePageErrors", "Capture future page errors.", "capturePageErrors"));
        command.Subcommands.Add(BuildMessageWaitCommand(browserOptions, "wait", "waitForPageError", "Wait for a matching page error.", includeLevel: false));
        command.Subcommands.Add(BuildMessageWaitCommand(browserOptions, "waitForPageError", "waitForPageError", "Wait for a matching page error.", includeLevel: false));
        command.Subcommands.Add(BuildNoPageErrorCommand(browserOptions, "expectNoPageError", "expectNoPageError"));
        command.Subcommands.Add(BuildNoPageErrorCommand(browserOptions, "toHaveNoPageError", "toHaveNoPageError"));
        return command;
    }

    private Command BuildDialogCaptureCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var promptText = CliStringOption("--prompt-text", "Prompt text to return when accepting prompts.");
        var command = new Command(name, "Install dialog capture with default accept behavior.") { promptText };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [], DialogOptions(parseResult, promptText))));

        return command;
    }

    private Command BuildDialogBehaviorCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var behavior = new Argument<string>("behavior") { Description = "Dialog behavior: accept or dismiss." };
        var promptText = CliStringOption("--prompt-text", "Prompt text to return when accepting prompts.");
        var command = new Command(name, "Set automated browser dialog behavior.") { behavior, promptText };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(behavior) ?? string.Empty], DialogOptions(parseResult, promptText))));

        return command;
    }

    private Command BuildMessageWaitCommand(BrowserSelectionOptions browserOptions, string name, string action, string description, bool includeLevel)
    {
        var text = new Argument<string>("text") { Description = "Message text to match." };
        var timeout = CliIntOption("--timeout", "Timeout in milliseconds.");
        var level = CliStringOption("--level", "Console level filter: log, info, warn, or error.");
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = includeLevel
            ? new Command(name, description) { text, timeout, level, match, ignoreCase }
            : new Command(name, description) { text, timeout, match, ignoreCase };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(text) ?? string.Empty], EventWaitOptions(parseResult, timeout, includeLevel ? level : null, match, ignoreCase))));

        return command;
    }

    private Command BuildNoConsoleCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var text = OptionalTextArgument("text", "Optional console text substring to reject.");
        var timeout = CliIntOption("--timeout", "Observation window in milliseconds.");
        var level = CliStringOption("--level", "Console level filter: log, info, warn, or error. Default is error.");
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command(name, "Assert that no matching console message is captured.") { text, timeout, level, match, ignoreCase };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, OptionalArgument(parseResult, text), EventWaitOptions(parseResult, timeout, level, match, ignoreCase))));

        return command;
    }

    private Command BuildNoPageErrorCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var text = OptionalTextArgument("text", "Optional page error text substring to reject.");
        var timeout = CliIntOption("--timeout", "Observation window in milliseconds.");
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command(name, "Assert that no matching page error is captured.") { text, timeout, match, ignoreCase };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, OptionalArgument(parseResult, text), EventWaitOptions(parseResult, timeout, null, match, ignoreCase))));

        return command;
    }

    private Command BuildWaitForEventCommand(BrowserSelectionOptions browserOptions, string name = "wait")
    {
        var eventName = new Argument<string>("event") { Description = "Event name, such as dialog, console, request, response, worker, websocket, websocketMessage, or download." };
        var matcher = new Argument<string?>("matcher") { Description = "Optional event matcher text.", Arity = ArgumentArity.ZeroOrOne };
        var command = new Command(name, "Wait for a supported browser event.") { eventName, matcher };
        var timeout = CliIntOption("--timeout", "Timeout in milliseconds.");
        var level = CliStringOption("--level", "Console level filter.");
        var count = CliIntOption("--count", "Expected tab or popup count.");
        var directory = new Option<DirectoryInfo?>("--directory") { Description = "Download directory." };
        var pattern = CliStringOption("--pattern", "Download file glob or URL/message matcher.");
        var url = CliStringOption("--url", "URL matcher alias for network, websocket, worker, or page events.");
        var message = CliStringOption("--message", "Message matcher alias for dialog, console, page-error, or websocket-message events.");
        var text = CliStringOption("--text", "Text matcher alias for dialog, console, and page-error events.");
        var method = CliStringOption("--method", "HTTP method filter.");
        var status = CliIntOption("--status", "HTTP status filter.");
        var contains = CliStringOption("--contains", "Body, response, or error text filter.");
        var mocked = CliStringOption("--mocked", "Whether to match mocked or real traffic: true or false.");
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        foreach (var option in new Option[] { timeout, level, count, directory, pattern, url, message, text, method, status, contains, mocked, match, ignoreCase })
        {
            command.Options.Add(option);
        }

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForEvent", EventArguments(parseResult, eventName, matcher), EventOptions(parseResult, timeout, level, count, directory, pattern, url, message, text, method, status, contains, mocked, match, ignoreCase))));

        return command;
    }

}

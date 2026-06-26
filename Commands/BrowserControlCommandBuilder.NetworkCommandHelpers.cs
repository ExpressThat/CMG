using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildNetworkNoArgumentCommand(BrowserSelectionOptions browserOptions, string name, string description, string action)
    {
        var command = new Command(name, description);
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            action));
        return command;
    }

    private Command BuildHarCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var path = new Option<FileInfo>("--path")
        {
            Description = "HAR file path.",
            Required = true
        };
        var command = new Command(action, description) { path };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [], [("path", parseResult.GetValue(path)?.FullName ?? string.Empty)])));
        return command;
    }

    private Command BuildHeadersCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var pairs = new Argument<string[]>("pairs")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "Header name/value pairs."
        };
        var command = new Command(name, description) { pairs };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, parseResult.GetValue(pairs) ?? [], [])));
        return command;
    }

    private Command BuildCredentialsCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var username = new Argument<string>("username") { Description = "HTTP auth username." };
        var password = new Argument<string>("password") { Description = "HTTP auth password." };
        var command = new Command(name, description) { username, password };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(username) ?? string.Empty, parseResult.GetValue(password) ?? string.Empty], [])));
        return command;
    }

    private Command BuildProxyCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var prefix = new Argument<string>("prefix") { Description = "Proxy URL prefix." };
        var command = new Command(name, description) { prefix };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, parseResult.GetValue(prefix) ?? string.Empty)));
        return command;
    }

    private Command BuildOfflineCommand(BrowserSelectionOptions browserOptions)
    {
        var enabled = new Argument<bool>("enabled") { Description = "true to simulate offline; false to restore." };
        var command = new Command("setOffline", "Enable or disable page-side offline simulation.") { enabled };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("setOffline", parseResult.GetValue(enabled).ToString().ToLowerInvariant())));
        return command;
    }

    private Command BuildWebSocketGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("webSocket", "WebSocket route and wait commands.");
        command.Subcommands.Add(BuildWebSocketRouteCommand(browserOptions));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearRoutes", "Clear WebSocket routes.", "clearWebSocketRoutes"));
        command.Subcommands.Add(BuildWebSocketWaitCommand(browserOptions, "wait", "waitForWebSocket", "Wait for a matching WebSocket."));
        command.Subcommands.Add(BuildWebSocketWaitCommand(browserOptions, "waitMessage", "waitForWebSocketMessage", "Wait for a matching WebSocket message."));
        return command;
    }

    private Command BuildWebSocketRouteCommand(BrowserSelectionOptions browserOptions)
    {
        var pattern = new Argument<string>("pattern") { Description = "URL pattern to match." };
        var message = new Option<string?>("--message") { Description = "Message to send after open." };
        var close = new Option<bool?>("--close") { Description = "Whether to close the socket." };
        var code = new Option<int?>("--code") { Description = "WebSocket close code." };
        var reason = new Option<string?>("--reason") { Description = "WebSocket close reason." };
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command("route", "Install a WebSocket route.") { pattern, message, close, code, reason, match, ignoreCase };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("routeWebSocket", [parseResult.GetValue(pattern) ?? string.Empty], CompactOptions([
                StringOption("message", parseResult.GetValue(message)),
                BoolOption("close", parseResult.GetValue(close)),
                IntOption("code", parseResult.GetValue(code)),
                StringOption("reason", parseResult.GetValue(reason)),
                StringOption("match", parseResult.GetValue(match)),
                parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
            ]))));
        return command;
    }

    private Command BuildWebSocketWaitCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var pattern = new Argument<string>("pattern") { Description = "URL or message pattern to match." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command(name, description) { pattern, timeout, match, ignoreCase };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(pattern) ?? string.Empty], CompactOptions([
                IntOption("timeout", parseResult.GetValue(timeout)),
                StringOption("match", parseResult.GetValue(match)),
                parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
            ]))));
        return command;
    }

    private static IReadOnlyList<(string Key, string Value)> WaitOptions(
        ParseResult parseResult,
        Option<int?> timeout,
        Option<string?> method,
        Option<int?> status,
        Option<string?> contains,
        Option<string?> mocked,
        Option<string?> header,
        Option<string?> headerName,
        Option<string?> headerValue,
        Option<string?> match,
        Option<bool> ignoreCase) =>
        CompactOptions([
            IntOption("timeout", parseResult.GetValue(timeout)),
            StringOption("method", parseResult.GetValue(method)),
            IntOption("status", parseResult.GetValue(status)),
            StringOption("contains", parseResult.GetValue(contains)),
            StringOption("mocked", parseResult.GetValue(mocked)),
            StringOption("header", parseResult.GetValue(header)),
            StringOption("headerName", parseResult.GetValue(headerName)),
            StringOption("headerValue", parseResult.GetValue(headerValue)),
            StringOption("match", parseResult.GetValue(match)),
            parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
        ]);

    private static (string Key, string Value)? StringOption(string key, string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : (key, value);

    private static (string Key, string Value)? IntOption(string key, int? value) =>
        value is null ? null : (key, value.Value.ToString());

    private static (string Key, string Value)? BoolOption(string key, bool? value) =>
        value is null ? null : (key, value.Value.ToString().ToLowerInvariant());

    private static IReadOnlyList<(string Key, string Value)> CompactOptions(IReadOnlyList<(string Key, string Value)?> values) =>
        values.Where(value => value is not null).Select(value => value!.Value).ToArray();
}

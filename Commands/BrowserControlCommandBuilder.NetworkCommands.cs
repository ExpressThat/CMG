using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildNetworkGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("network", "Network routing, waits, HAR, and environment commands.");

        command.Subcommands.Add(BuildRouteCommand(browserOptions, "route", "Install a fetch/XHR route."));
        command.Subcommands.Add(BuildRouteCommand(browserOptions, "intercept", "Alias a fetch/XHR route as an intercept."));
        command.Subcommands.Add(BuildRouteCommand(browserOptions, "mockResponse", "Alias a fetch/XHR route as a mocked response."));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearRoutes", "Clear network routes.", "clearRoutes"));
        command.Subcommands.Add(BuildNetworkWaitCommand(browserOptions, "waitForRequest", "Wait for a matching request."));
        command.Subcommands.Add(BuildNetworkWaitCommand(browserOptions, "waitForRequestFinished", "Wait for a matching completed request."));
        command.Subcommands.Add(BuildNetworkWaitCommand(browserOptions, "waitForRequestFailed", "Wait for a matching failed request."));
        command.Subcommands.Add(BuildNetworkWaitCommand(browserOptions, "waitForResponse", "Wait for a matching response."));
        command.Subcommands.Add(BuildHarCommand(browserOptions, "exportHar", "Export recorded page network traffic."));
        command.Subcommands.Add(BuildHarCommand(browserOptions, "replayHar", "Replay responses from a HAR file."));
        command.Subcommands.Add(BuildHeadersCommand(browserOptions));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearHeaders", "Clear extra HTTP headers.", "clearExtraHTTPHeaders"));
        command.Subcommands.Add(BuildCredentialsCommand(browserOptions));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearCredentials", "Clear HTTP credentials.", "clearHttpCredentials"));
        command.Subcommands.Add(BuildProxyCommand(browserOptions));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "clearProxy", "Clear the page-side proxy rewrite."));
        command.Subcommands.Add(BuildOfflineCommand(browserOptions));
        command.Subcommands.Add(BuildWebSocketGroup(browserOptions));

        return command;
    }

    private Command BuildRouteCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var pattern = new Argument<string>("pattern") { Description = "URL substring to match." };
        var status = new Option<int?>("--status") { Description = "Mocked response status." };
        var body = new Option<string?>("--body") { Description = "Mocked response body." };
        var contentType = new Option<string?>("--content-type") { Description = "Mocked response content type." };
        var method = new Option<string?>("--method") { Description = "HTTP method filter." };
        var times = new Option<int?>("--times") { Description = "Remove route after this many matches." };
        var delay = new Option<int?>("--delay") { Description = "Response delay in milliseconds." };
        var abort = new Option<bool>("--abort") { Description = "Abort matching requests." };
        var command = new Command(name, description) { pattern, status, body, contentType, method, times, delay, abort };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, [parseResult.GetValue(pattern) ?? string.Empty], RouteOptions(parseResult, status, body, contentType, method, times, delay, abort))));
        return command;
    }

    private Command BuildNetworkWaitCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var pattern = new Argument<string>("pattern") { Description = "URL substring to match." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var method = new Option<string?>("--method") { Description = "HTTP method filter." };
        var status = new Option<int?>("--status") { Description = "HTTP status filter." };
        var contains = new Option<string?>("--contains") { Description = "Body, response, or error text filter." };
        var mocked = new Option<string?>("--mocked") { Description = "Whether to match mocked or real traffic: true or false." };
        var command = new Command(name, description) { pattern, timeout, method, status, contains, mocked };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, [parseResult.GetValue(pattern) ?? string.Empty], WaitOptions(parseResult, timeout, method, status, contains, mocked))));
        return command;
    }

    private static IReadOnlyList<(string Key, string Value)> RouteOptions(
        ParseResult parseResult,
        Option<int?> status,
        Option<string?> body,
        Option<string?> contentType,
        Option<string?> method,
        Option<int?> times,
        Option<int?> delay,
        Option<bool> abort) =>
        CompactOptions([
            IntOption("status", parseResult.GetValue(status)),
            StringOption("body", parseResult.GetValue(body)),
            StringOption("contentType", parseResult.GetValue(contentType)),
            StringOption("method", parseResult.GetValue(method)),
            IntOption("times", parseResult.GetValue(times)),
            IntOption("delay", parseResult.GetValue(delay)),
            parseResult.GetValue(abort) ? ("abort", "true") : null
        ]);
}

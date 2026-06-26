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
        command.Subcommands.Add(BuildHeadersCommand(browserOptions, "setHeaders", "setExtraHTTPHeaders", "Set extra HTTP headers."));
        command.Subcommands.Add(BuildHeadersCommand(browserOptions, "setExtraHTTPHeaders", "setExtraHTTPHeaders", "Set extra HTTP headers."));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearHeaders", "Clear extra HTTP headers.", "clearExtraHTTPHeaders"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearExtraHTTPHeaders", "Clear extra HTTP headers.", "clearExtraHTTPHeaders"));
        command.Subcommands.Add(BuildCredentialsCommand(browserOptions, "setCredentials", "setHttpCredentials", "Set page-side HTTP credentials."));
        command.Subcommands.Add(BuildCredentialsCommand(browserOptions, "setHttpCredentials", "setHttpCredentials", "Set page-side HTTP credentials."));
        command.Subcommands.Add(BuildCredentialsCommand(browserOptions, "httpCredentials", "setHttpCredentials", "Set page-side HTTP credentials."));
        command.Subcommands.Add(BuildCredentialsCommand(browserOptions, "authenticate", "setHttpCredentials", "Set page-side HTTP credentials."));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearCredentials", "Clear HTTP credentials.", "clearHttpCredentials"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearHttpCredentials", "Clear HTTP credentials.", "clearHttpCredentials"));
        command.Subcommands.Add(BuildProxyCommand(browserOptions, "setProxy", "setProxy", "Set a page-side fetch/XHR proxy rewrite."));
        command.Subcommands.Add(BuildProxyCommand(browserOptions, "proxy", "setProxy", "Set a page-side fetch/XHR proxy rewrite."));
        command.Subcommands.Add(BuildNoArgumentScriptCommand(browserOptions, "clearProxy", "Clear the page-side proxy rewrite."));
        command.Subcommands.Add(BuildOfflineCommand(browserOptions));
        command.Subcommands.Add(BuildWebSocketGroup(browserOptions));

        return command;
    }

    private Command BuildRouteCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var pattern = new Argument<string>("pattern") { Description = "URL text to match." };
        var status = new Option<int?>("--status") { Description = "Mocked response status." };
        var body = new Option<string?>("--body") { Description = "Mocked response body." };
        var contentType = new Option<string?>("--content-type") { Description = "Mocked response content type." };
        var method = new Option<string?>("--method") { Description = "HTTP method filter." };
        var times = new Option<int?>("--times") { Description = "Remove route after this many matches." };
        var delay = new Option<int?>("--delay") { Description = "Response delay in milliseconds." };
        var abort = new Option<bool>("--abort") { Description = "Abort matching requests." };
        var header = new Option<string?>("--header") { Description = "Response header as Name: value." };
        var headers = new Option<string?>("--headers") { Description = "Response headers separated by semicolons." };
        var headerName = new Option<string?>("--header-name") { Description = "Response header name." };
        var headerValue = new Option<string?>("--header-value") { Description = "Response header value." };
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command(name, description)
        {
            pattern,
            status,
            body,
            contentType,
            method,
            times,
            delay,
            abort,
            header,
            headers,
            headerName,
            headerValue,
            match,
            ignoreCase
        };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, [parseResult.GetValue(pattern) ?? string.Empty], RouteOptions(parseResult, status, body, contentType, method, times, delay, abort, header, headers, headerName, headerValue, match, ignoreCase))));
        return command;
    }

    private Command BuildNetworkWaitCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var pattern = new Argument<string>("pattern") { Description = "URL text to match." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds." };
        var method = new Option<string?>("--method") { Description = "HTTP method filter." };
        var status = new Option<int?>("--status") { Description = "HTTP status filter." };
        var contains = new Option<string?>("--contains") { Description = "Body, response, or error text filter." };
        var mocked = new Option<string?>("--mocked") { Description = "Whether to match mocked or real traffic: true or false." };
        var header = new Option<string?>("--header") { Description = "Header filter as Name or Name: value." };
        var headerName = new Option<string?>("--header-name") { Description = "Header name filter." };
        var headerValue = new Option<string?>("--header-value") { Description = "Header value substring filter." };
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var command = new Command(name, description)
        {
            pattern,
            timeout,
            method,
            status,
            contains,
            mocked,
            header,
            headerName,
            headerValue,
            match,
            ignoreCase
        };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, [parseResult.GetValue(pattern) ?? string.Empty], WaitOptions(parseResult, timeout, method, status, contains, mocked, header, headerName, headerValue, match, ignoreCase))));
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
        Option<bool> abort,
        Option<string?> header,
        Option<string?> headers,
        Option<string?> headerName,
        Option<string?> headerValue,
        Option<string?> match,
        Option<bool> ignoreCase) =>
        CompactOptions([
            IntOption("status", parseResult.GetValue(status)),
            StringOption("body", parseResult.GetValue(body)),
            StringOption("contentType", parseResult.GetValue(contentType)),
            StringOption("method", parseResult.GetValue(method)),
            IntOption("times", parseResult.GetValue(times)),
            IntOption("delay", parseResult.GetValue(delay)),
            parseResult.GetValue(abort) ? ("abort", "true") : null,
            StringOption("header", parseResult.GetValue(header)),
            StringOption("headers", parseResult.GetValue(headers)),
            StringOption("headerName", parseResult.GetValue(headerName)),
            StringOption("headerValue", parseResult.GetValue(headerValue)),
            StringOption("match", parseResult.GetValue(match)),
            parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
        ]);
}

using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildContextGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("context", "Browser context, emulation, permission, and environment commands.");

        command.Subcommands.Add(BuildEmulateCommand(browserOptions));
        command.Subcommands.Add(BuildGeolocationCommand(browserOptions));
        command.Subcommands.Add(BuildGrantPermissionsCommand(browserOptions));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearPermissions", "Clear page-side permission grants.", "clearPermissions"));
        command.Subcommands.Add(BuildBooleanContextCommand(browserOptions, "setJavaScriptEnabled", "Enable or disable dynamic JavaScript execution."));
        command.Subcommands.Add(BuildBooleanContextCommand(browserOptions, "javaScriptEnabled", "Enable or disable dynamic JavaScript execution."));
        command.Subcommands.Add(BuildBooleanContextCommand(browserOptions, "bypassCSP", "Enable or disable page-side CSP bypass."));
        command.Subcommands.Add(BuildServiceWorkersCommand(browserOptions));
        command.Subcommands.Add(BuildServiceWorkersCommand(browserOptions, "setServiceWorkers"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clear", "Clear storage, cookies, caches, IndexedDB, and service workers.", "clearContext"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "clearContext", "Clear storage, cookies, caches, IndexedDB, and service workers.", "clearContext"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "reset", "Clear context state and navigate to about:blank.", "resetContext"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "resetContext", "Clear context state and navigate to about:blank.", "resetContext"));
        command.Subcommands.Add(BuildBrowserContextsGroup(browserOptions));

        return command;
    }

    private Command BuildEmulateCommand(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("emulate", "Apply page environment and viewport emulation.");
        var width = new Option<int?>("--width") { Description = "Viewport width in CSS pixels." };
        var height = new Option<int?>("--height") { Description = "Viewport height in CSS pixels." };
        var scale = new Option<double?>("--device-scale-factor") { Description = "Viewport device scale factor." };
        var mobile = new Option<bool>("--mobile") { Description = "Use mobile viewport metrics." };
        var touch = new Option<bool>("--touch") { Description = "Enable touch viewport hints." };
        var userAgent = CliStringOption("--user-agent", "Page-visible navigator.userAgent value.");
        var locale = CliStringOption("--locale", "Page-visible locale, such as en-GB.");
        var timezone = CliStringOption("--timezone", "Reported IANA timezone.");
        var colorScheme = CliStringOption("--color-scheme", "Preferred color scheme: light or dark.");
        var reducedMotion = CliStringOption("--reduced-motion", "Reduced motion value: reduce or no-preference.");
        var geolocation = CliStringOption("--geolocation", "Stubbed coordinates as latitude,longitude.");
        var permissions = CliStringOption("--permissions", "Comma-separated granted permission names.");

        foreach (var option in new Option[] { width, height, scale, mobile, touch, userAgent, locale, timezone, colorScheme, reducedMotion, geolocation, permissions })
        {
            command.Options.Add(option);
        }

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("emulate", [], EmulateOptions(parseResult, width, height, scale, mobile, touch, userAgent, locale, timezone, colorScheme, reducedMotion, geolocation, permissions))));

        return command;
    }

    private Command BuildGeolocationCommand(BrowserSelectionOptions browserOptions)
    {
        var latitude = new Argument<double>("latitude") { Description = "Latitude." };
        var longitude = new Argument<double>("longitude") { Description = "Longitude." };
        var accuracy = new Option<double?>("--accuracy") { Description = "Coordinate accuracy in meters. Default is 1." };
        var command = new Command("setGeolocation", "Set page-visible geolocation.") { latitude, longitude, accuracy };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("setGeolocation", [], CompactOptions([
                StringOption("latitude", parseResult.GetValue(latitude).ToString()),
                StringOption("longitude", parseResult.GetValue(longitude).ToString()),
                StringOption("accuracy", parseResult.GetValue(accuracy)?.ToString())
            ]))));

        return command;
    }

    private Command BuildGrantPermissionsCommand(BrowserSelectionOptions browserOptions)
    {
        var permissions = new Argument<string[]>("permissions")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "Permission names to grant."
        };
        var command = new Command("grantPermissions", "Grant page-side permissions.") { permissions };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("grantPermissions", parseResult.GetValue(permissions) ?? [], [])));

        return command;
    }

    private Command BuildBooleanContextCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        var enabled = new Argument<bool>("enabled") { Description = "true to enable, false to disable." };
        var command = new Command(name, description) { enabled };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, parseResult.GetValue(enabled).ToString().ToLowerInvariant())));
        return command;
    }

    private Command BuildServiceWorkersCommand(BrowserSelectionOptions browserOptions)
    {
        return BuildServiceWorkersCommand(browserOptions, "serviceWorkers");
    }

    private Command BuildServiceWorkersCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var mode = new Argument<string>("mode") { Description = "Service worker mode: allow or block." };
        var command = new Command(name, "Allow or block service worker registration.") { mode };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(name, parseResult.GetValue(mode) ?? string.Empty)));
        return command;
    }

    private Command BuildBrowserContextsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("browserContexts", "Isolated browser context commands.");
        command.Subcommands.Add(BuildNewBrowserContextCommand(browserOptions));
        command.Subcommands.Add(BuildNewBrowserContextCommand(browserOptions, "newContext"));
        command.Subcommands.Add(BuildContextIdCommand(browserOptions, "use", "Activate a browser context by id or target id.", "useContext"));
        command.Subcommands.Add(BuildContextIdCommand(browserOptions, "useContext", "Activate a browser context by id or target id.", "useContext"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "list", "List browser contexts.", "listContexts"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "listContexts", "List browser contexts.", "listContexts"));
        command.Subcommands.Add(BuildContextIdCommand(browserOptions, "close", "Close a browser context by id or target id.", "closeContext"));
        command.Subcommands.Add(BuildContextIdCommand(browserOptions, "closeContext", "Close a browser context by id or target id.", "closeContext"));
        return command;
    }

    private Command BuildNewBrowserContextCommand(BrowserSelectionOptions browserOptions)
    {
        return BuildNewBrowserContextCommand(browserOptions, "new");
    }

    private Command BuildNewBrowserContextCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var url = CliStringOption("--url", "Initial URL. Default is about:blank.");
        var command = new Command(name, "Create and activate a fresh browser context.") { url };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("newContext", [], CompactOptions([StringOption("url", parseResult.GetValue(url))]))));
        return command;
    }

    private Command BuildContextIdCommand(BrowserSelectionOptions browserOptions, string name, string description, string action)
    {
        var id = new Argument<string>("id") { Description = "Context id or target id." };
        var command = new Command(name, description) { id };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, parseResult.GetValue(id) ?? string.Empty)));
        return command;
    }

    private static IReadOnlyList<(string Key, string Value)> EmulateOptions(
        ParseResult parseResult,
        Option<int?> width,
        Option<int?> height,
        Option<double?> scale,
        Option<bool> mobile,
        Option<bool> touch,
        Option<string?> userAgent,
        Option<string?> locale,
        Option<string?> timezone,
        Option<string?> colorScheme,
        Option<string?> reducedMotion,
        Option<string?> geolocation,
        Option<string?> permissions) =>
        CompactOptions([
            IntOption("width", parseResult.GetValue(width)),
            IntOption("height", parseResult.GetValue(height)),
            StringOption("deviceScaleFactor", parseResult.GetValue(scale)?.ToString()),
            BoolOption("isMobile", parseResult.GetValue(mobile) ? true : null),
            BoolOption("hasTouch", parseResult.GetValue(touch) ? true : null),
            StringOption("userAgent", parseResult.GetValue(userAgent)),
            StringOption("locale", parseResult.GetValue(locale)),
            StringOption("timezone", parseResult.GetValue(timezone)),
            StringOption("colorScheme", parseResult.GetValue(colorScheme)),
            StringOption("reducedMotion", parseResult.GetValue(reducedMotion)),
            StringOption("geolocation", parseResult.GetValue(geolocation)),
            StringOption("permissions", parseResult.GetValue(permissions))
        ]);
}

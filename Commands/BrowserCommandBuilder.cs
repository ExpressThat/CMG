using CMG.Browser;
using System.CommandLine;

namespace CMG.Commands;

public sealed class BrowserCommandBuilder
{
    private readonly IBrowserCommandHandler browserCommandHandler;
    private readonly BrowserControlCommandBuilder browserControlCommandBuilder;

    public BrowserCommandBuilder(
        IBrowserCommandHandler browserCommandHandler,
        BrowserControlCommandBuilder browserControlCommandBuilder)
    {
        this.browserCommandHandler = browserCommandHandler;
        this.browserControlCommandBuilder = browserControlCommandBuilder;
    }

    public Command Build(BrowserSelectionOptions browserOptions)
    {
        var portOption = new Option<int?>("--port")
        {
            Description = "Remote debugging port for the browser instance. Defaults to the selected browser default."
        };
        var scopedBrowserOptions = browserOptions with { Port = portOption };
        var browserCommand = new Command("browser", "Browser lifecycle and capture commands.");
        browserCommand.Options.Add(portOption);

        browserCommand.Subcommands.Add(BuildLaunchCommand(scopedBrowserOptions));
        browserCommand.Subcommands.Add(BuildAppCommand(scopedBrowserOptions));
        browserCommand.Subcommands.Add(BuildCloseCommand(scopedBrowserOptions));
        browserCommand.Subcommands.Add(browserControlCommandBuilder.Build(scopedBrowserOptions));

        return browserCommand;
    }

    private Command BuildLaunchCommand(BrowserSelectionOptions browserOptions)
    {
        var arguments = CreateTrailingArguments("Additional browser launch arguments.");
        var headlessOption = new Option<bool>("--headless")
        {
            Description = "Launch the browser in headless mode."
        };
        var urlOption = new Option<string?>("--url")
        {
            Description = "Initial URL or path to open."
        };
        var command = new Command("launch", "Launch a browser instance.")
        {
            arguments,
            headlessOption,
            urlOption
        };

        command.SetAction(parseResult =>
        {
            var values = parseResult.GetValue(arguments) ?? [];
            return browserCommandHandler.Launch(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                values,
                parseResult.GetValue(headlessOption),
                parseResult.GetValue(urlOption),
                CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions));
        });

        return command;
    }

    private Command BuildAppCommand(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("app", "Attach CMG to Chromium-based desktop apps.");

        command.Subcommands.Add(BuildAppLaunchCommand(browserOptions));
        command.Subcommands.Add(BuildAppAttachCommand(browserOptions));

        return command;
    }

    private Command BuildAppLaunchCommand(BrowserSelectionOptions browserOptions)
    {
        var executable = new Argument<FileInfo>("executable")
        {
            Description = "Electron or WebView2 application executable."
        };
        var arguments = CreateTrailingArguments("Additional app launch arguments.");
        var kind = new Option<string>("--kind")
        {
            Description = "App engine: electron or webview2.",
            DefaultValueFactory = _ => "electron"
        };
        var port = new Option<int>("--port")
        {
            Description = "Remote debugging port to expose.",
            DefaultValueFactory = _ => 9222
        };
        var host = CreateHostOption();
        var timeout = CreateConnectTimeoutOption();

        var command = new Command("launch", "Launch an Electron or Windows WebView2 app with debugging enabled.")
        {
            executable,
            arguments,
            kind,
            port,
            host,
            timeout
        };

        command.SetAction(parseResult =>
        {
            return browserCommandHandler.LaunchApp(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(executable)!,
                parseResult.GetValue(kind) ?? "electron",
                new BrowserAppDebugOptions(
                    parseResult.GetValue(port),
                    parseResult.GetValue(host) ?? "127.0.0.1",
                    parseResult.GetValue(timeout)),
                parseResult.GetValue(arguments) ?? []);
        });

        return command;
    }

    private Command BuildAppAttachCommand(BrowserSelectionOptions browserOptions)
    {
        var port = new Option<int>("--port")
        {
            Description = "Remote debugging port already exposed by the app.",
            DefaultValueFactory = _ => 9222
        };
        var pid = new Option<int>("--pid")
        {
            Description = "Optional app process id for later close tracking.",
            DefaultValueFactory = _ => 0
        };
        var host = CreateHostOption();
        var timeout = CreateConnectTimeoutOption();

        var command = new Command("attach", "Use an existing Electron or WebView2 debugging endpoint.")
        {
            port,
            pid,
            host,
            timeout
        };

        command.SetAction(parseResult =>
        {
            return browserCommandHandler.AttachApp(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                new BrowserAppDebugOptions(
                    parseResult.GetValue(port),
                    parseResult.GetValue(host) ?? "127.0.0.1",
                    parseResult.GetValue(timeout)),
                parseResult.GetValue(pid));
        });

        return command;
    }

    private Command BuildCloseCommand(BrowserSelectionOptions browserOptions)
    {
        var arguments = CreateTrailingArguments("Additional browser close arguments.");
        var command = new Command("close", "Close the active browser instance.")
        {
            arguments
        };

        command.SetAction(parseResult =>
        {
            var values = parseResult.GetValue(arguments) ?? [];
            return browserCommandHandler.Close(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                values,
                CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions));
        });

        return command;
    }

    private static Argument<string[]> CreateTrailingArguments(string description)
    {
        return new Argument<string[]>("arguments")
        {
            Arity = ArgumentArity.ZeroOrMore,
            Description = description
        };
    }

    private static Option<string> CreateHostOption()
    {
        return new Option<string>("--host")
        {
            Description = "Remote debugging host.",
            DefaultValueFactory = _ => "127.0.0.1"
        };
    }

    private static Option<int> CreateConnectTimeoutOption()
    {
        return new Option<int>("--connect-timeout")
        {
            Description = "Milliseconds to wait for the debugging endpoint. Use 0 to skip verification.",
            DefaultValueFactory = _ => 10_000
        };
    }
}


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
        var browserCommand = new Command("browser", "Browser lifecycle and capture commands.");

        browserCommand.Subcommands.Add(BuildLaunchCommand(browserOptions));
        browserCommand.Subcommands.Add(BuildAppCommand(browserOptions));
        browserCommand.Subcommands.Add(BuildCloseCommand(browserOptions));
        browserCommand.Subcommands.Add(browserControlCommandBuilder.Build(browserOptions));

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
                parseResult.GetValue(urlOption));
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

        var command = new Command("launch", "Launch an Electron or Windows WebView2 app with debugging enabled.")
        {
            executable,
            arguments,
            kind,
            port
        };

        command.SetAction(parseResult =>
        {
            return browserCommandHandler.LaunchApp(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(executable)!,
                parseResult.GetValue(kind) ?? "electron",
                parseResult.GetValue(port),
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

        var command = new Command("attach", "Use an existing Electron or WebView2 debugging endpoint.")
        {
            port,
            pid
        };

        command.SetAction(parseResult =>
        {
            return browserCommandHandler.AttachApp(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(port),
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
            return browserCommandHandler.Close(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), values);
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
}


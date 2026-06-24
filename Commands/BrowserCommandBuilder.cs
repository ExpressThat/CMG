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
        browserCommand.Subcommands.Add(BuildCloseCommand(browserOptions));
        browserCommand.Subcommands.Add(browserControlCommandBuilder.Build(browserOptions));

        return browserCommand;
    }

    private Command BuildLaunchCommand(BrowserSelectionOptions browserOptions)
    {
        var arguments = CreateTrailingArguments("Additional browser launch arguments.");

        var command = new Command("launch", "Launch a browser instance.")
        {
            arguments
        };

        command.SetAction(parseResult =>
        {
            var values = parseResult.GetValue(arguments) ?? [];
            return browserCommandHandler.Launch(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), values);
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


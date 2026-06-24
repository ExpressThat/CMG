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

    public Command Build(Option<bool> firefoxOption)
    {
        var browserCommand = new Command("browser", "Browser lifecycle and capture commands.");

        browserCommand.Subcommands.Add(BuildLaunchCommand(firefoxOption));
        browserCommand.Subcommands.Add(BuildCloseCommand(firefoxOption));
        browserCommand.Subcommands.Add(browserControlCommandBuilder.Build(firefoxOption));

        return browserCommand;
    }

    private Command BuildLaunchCommand(Option<bool> firefoxOption)
    {
        var arguments = CreateTrailingArguments("Additional browser launch arguments.");

        var command = new Command("launch", "Launch a browser instance.")
        {
            arguments
        };

        command.SetAction(parseResult =>
        {
            var values = parseResult.GetValue(arguments) ?? [];
            return browserCommandHandler.Launch(CommandTreeBuilder.GetBrowserKind(parseResult, firefoxOption), values);
        });

        return command;
    }

    private Command BuildCloseCommand(Option<bool> firefoxOption)
    {
        var arguments = CreateTrailingArguments("Additional browser close arguments.");

        var command = new Command("close", "Close the active browser instance.")
        {
            arguments
        };

        command.SetAction(parseResult =>
        {
            var values = parseResult.GetValue(arguments) ?? [];
            return browserCommandHandler.Close(CommandTreeBuilder.GetBrowserKind(parseResult, firefoxOption), values);
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

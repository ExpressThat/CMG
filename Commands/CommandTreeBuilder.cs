using System.CommandLine;

namespace CMG.Commands;

public interface ICommandTreeBuilder
{
    RootCommand Build();
}

public sealed class CommandTreeBuilder : ICommandTreeBuilder
{
    private readonly BrowserCommandBuilder browserCommandBuilder;

    public CommandTreeBuilder(BrowserCommandBuilder browserCommandBuilder)
    {
        this.browserCommandBuilder = browserCommandBuilder;
    }

    public RootCommand Build()
    {
        var rootCommand = new RootCommand("Control and capture browser screenshots and GIFs.");

        rootCommand.Subcommands.Add(browserCommandBuilder.Build());

        return rootCommand;
    }
}

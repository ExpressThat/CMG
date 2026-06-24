using System.CommandLine;
using CMG.Browser;

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
        var firefoxOption = new Option<bool>("--firefox")
        {
            Description = "Use Firefox instead of the default Chrome browser."
        };
        rootCommand.Options.Add(firefoxOption);

        rootCommand.Subcommands.Add(browserCommandBuilder.Build(firefoxOption));

        return rootCommand;
    }

    public static BrowserKind GetBrowserKind(ParseResult parseResult, Option<bool> firefoxOption) =>
        parseResult.GetValue(firefoxOption) ? BrowserKind.Firefox : BrowserKind.Chrome;
}

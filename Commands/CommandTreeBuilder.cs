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
        var chromeOption = new Option<bool>("--chrome")
        {
            Description = "Use Chrome. This is the default when no browser option is provided."
        };
        var edgeOption = new Option<bool>("--edge")
        {
            Description = "Use Microsoft Edge instead of the default Chrome browser."
        };
        var firefoxOption = new Option<bool>("--firefox")
        {
            Description = "Use Firefox instead of the default Chrome browser."
        };
        var browserOptions = new BrowserSelectionOptions(chromeOption, edgeOption, firefoxOption);
        rootCommand.Options.Add(chromeOption);
        rootCommand.Options.Add(edgeOption);
        rootCommand.Options.Add(firefoxOption);

        rootCommand.Subcommands.Add(browserCommandBuilder.Build(browserOptions));

        return rootCommand;
    }

    public static BrowserKind GetBrowserKind(ParseResult parseResult, BrowserSelectionOptions browserOptions)
    {
        var selectedOptions = parseResult.Tokens
            .Select(token => token.Value)
            .Where(value => value is "--chrome" or "--edge" or "--firefox")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (selectedOptions.Length > 1)
        {
            return BrowserKind.InvalidSelection;
        }

        if (parseResult.GetValue(browserOptions.Edge))
        {
            return BrowserKind.Edge;
        }

        return parseResult.GetValue(browserOptions.Firefox) ? BrowserKind.Firefox : BrowserKind.Chrome;
    }
}

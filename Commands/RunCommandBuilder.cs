using System.CommandLine;
using CMG.Browser;
using CMG.Runner;

namespace CMG.Commands;

public sealed class RunCommandBuilder
{
    private readonly ICmgRunCommandHandler handler;

    public RunCommandBuilder(ICmgRunCommandHandler handler)
    {
        this.handler = handler;
    }

    public Command Build(BrowserSelectionOptions browserOptions)
    {
        var pathArgument = new Argument<string>("path")
        {
            Description = "A CMG script file or folder containing .cmgscript files."
        };
        var gifOption = new Option<DirectoryInfo?>("--gif")
        {
            Description = "Write per-test GIF recordings to this directory."
        };
        gifOption.Aliases.Add("-gif");
        var jsonOption = new Option<FileInfo?>("--report-json")
        {
            Description = "Write a JSON test report to this file."
        };
        var htmlOption = new Option<FileInfo?>("--report-html")
        {
            Description = "Write an HTML test report to this file."
        };
        var junitOption = new Option<FileInfo?>("--report-junit")
        {
            Description = "Write a JUnit XML test report to this file."
        };
        var command = new Command("run", "Run CMG DSL tests with visual artifacts.")
        {
            pathArgument,
            gifOption,
            jsonOption,
            htmlOption,
            junitOption
        };

        command.SetAction(parseResult =>
            handler.Run(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(pathArgument) ?? string.Empty,
                parseResult.GetValue(gifOption),
                parseResult.GetValue(jsonOption),
                parseResult.GetValue(htmlOption),
                parseResult.GetValue(junitOption)));

        return command;
    }
}

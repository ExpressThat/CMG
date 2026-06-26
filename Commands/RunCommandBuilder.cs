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
        var traceOption = new Option<DirectoryInfo?>("--trace")
        {
            Description = "Write per-test trace JSON files to this directory."
        };
        var grepOption = new Option<string?>("--grep")
        {
            Description = "Run tests whose names contain this text."
        };
        var tagOption = new Option<string?>("--tag")
        {
            Description = "Run tests with a matching tag option."
        };
        var retriesOption = new Option<int>("--retries")
        {
            Description = "Retry failed tests this many times.",
            DefaultValueFactory = _ => 0
        };
        var maxFailuresOption = new Option<int>("--max-failures")
        {
            Description = "Stop the run after this many failed tests.",
            DefaultValueFactory = _ => 0
        };
        var shardOption = new Option<string?>("--shard")
        {
            Description = "Run one shard as index/count, for example 1/3."
        };
        var command = new Command("run", "Run CMG DSL tests with visual artifacts.")
        {
            pathArgument,
            gifOption,
            jsonOption,
            htmlOption,
            junitOption,
            traceOption,
            grepOption,
            tagOption,
            retriesOption,
            maxFailuresOption,
            shardOption
        };

        command.SetAction(parseResult =>
            handler.Run(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(pathArgument) ?? string.Empty,
                parseResult.GetValue(gifOption),
                parseResult.GetValue(jsonOption),
                parseResult.GetValue(htmlOption),
                parseResult.GetValue(junitOption),
                parseResult.GetValue(traceOption),
                parseResult.GetValue(grepOption),
                parseResult.GetValue(tagOption),
                parseResult.GetValue(retriesOption),
                parseResult.GetValue(maxFailuresOption),
                parseResult.GetValue(shardOption)));

        return command;
    }
}

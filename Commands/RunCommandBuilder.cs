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
        var repeatEachOption = new Option<int>("--repeat-each")
        {
            Description = "Run each selected test this many times.",
            DefaultValueFactory = _ => 1
        };
        var listOption = new Option<bool>("--list")
        {
            Description = "List selected tests without connecting to a browser."
        };
        var shardOption = new Option<string?>("--shard")
        {
            Description = "Run one shard as index/count, for example 1/3."
        };
        var timeoutOption = new Option<int?>("--timeout")
        {
            Description = "Default timeout in milliseconds for timeout-capable actions."
        };
        var navigationTimeoutOption = new Option<int?>("--navigation-timeout")
        {
            Description = "Default timeout in milliseconds for navigation actions."
        };
        var assertionTimeoutOption = new Option<int?>("--assertion-timeout")
        {
            Description = "Default timeout in milliseconds for assertion actions."
        };
        var variableOption = new Option<string[]>("--var")
        {
            Description = "Initial runner variable as name=value. Can be repeated."
        };
        var envOption = new Option<string[]>("--env")
        {
            Description = "Alias for --var, useful for CI or agent-provided values."
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
            repeatEachOption,
            listOption,
            shardOption,
            timeoutOption,
            navigationTimeoutOption,
            assertionTimeoutOption,
            variableOption,
            envOption
        };

        command.SetAction(parseResult =>
        {
            var variableValues = (parseResult.GetValue(variableOption) ?? [])
                .Concat(parseResult.GetValue(envOption) ?? []);
            if (!VariableOptionParser.TryParse(variableValues, out var variables, out var error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }

            return
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
                parseResult.GetValue(repeatEachOption),
                parseResult.GetValue(listOption),
                parseResult.GetValue(shardOption),
                parseResult.GetValue(timeoutOption),
                parseResult.GetValue(navigationTimeoutOption),
                parseResult.GetValue(assertionTimeoutOption),
                variables);
        });

        return command;
    }
}

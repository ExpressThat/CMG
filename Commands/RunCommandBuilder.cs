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
        var configOption = new Option<FileInfo?>("--config")
        {
            Description = "JSON run config file. CLI options override config values."
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
        var baseUrlOption = new Option<string?>("--base-url")
        {
            Description = "Base URL used to resolve relative navigation targets."
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
            configOption,
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
            baseUrlOption,
            variableOption,
            envOption
        };

        command.SetAction(parseResult =>
        {
            if (!RunConfigReader.TryRead(parseResult.GetValue(configOption), out var config, out var configError))
            {
                Console.Error.WriteLine(configError);
                return 1;
            }

            var variableValues = (parseResult.GetValue(variableOption) ?? [])
                .Concat(parseResult.GetValue(envOption) ?? []);
            if (!VariableOptionParser.TryParse(variableValues, out var variables, out var error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }
            variables = MergeVariables(config.Variables, variables);

            return
            handler.Run(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(pathArgument) ?? string.Empty,
                DirectoryValue(parseResult, gifOption, config.Gif),
                FileValue(parseResult, jsonOption, config.ReportJson),
                FileValue(parseResult, htmlOption, config.ReportHtml),
                FileValue(parseResult, junitOption, config.ReportJunit),
                DirectoryValue(parseResult, traceOption, config.Trace),
                StringValue(parseResult, grepOption, config.Grep),
                StringValue(parseResult, tagOption, config.Tag),
                IntValue(parseResult, retriesOption, config.Retries),
                IntValue(parseResult, maxFailuresOption, config.MaxFailures),
                IntValue(parseResult, repeatEachOption, config.RepeatEach),
                parseResult.GetValue(listOption),
                StringValue(parseResult, shardOption, config.Shard),
                IntValue(parseResult, timeoutOption, config.Timeout),
                IntValue(parseResult, navigationTimeoutOption, config.NavigationTimeout),
                IntValue(parseResult, assertionTimeoutOption, config.AssertionTimeout),
                StringValue(parseResult, baseUrlOption, config.BaseUrl),
                variables);
        });

        return command;
    }

    private static bool WasProvided(ParseResult parseResult, Option option)
    {
        var names = option.Aliases.Prepend(option.Name).ToArray();
        return parseResult.Tokens.Any(token => names.Contains(token.Value, StringComparer.Ordinal));
    }

    private static DirectoryInfo? DirectoryValue(ParseResult parseResult, Option<DirectoryInfo?> option, DirectoryInfo? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static FileInfo? FileValue(ParseResult parseResult, Option<FileInfo?> option, FileInfo? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static string? StringValue(ParseResult parseResult, Option<string?> option, string? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static int IntValue(ParseResult parseResult, Option<int> option, int? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback ?? parseResult.GetValue(option);

    private static int? IntValue(ParseResult parseResult, Option<int?> option, int? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static IReadOnlyDictionary<string, string> MergeVariables(
        IReadOnlyDictionary<string, string> configVariables,
        IReadOnlyDictionary<string, string> cliVariables)
    {
        var merged = new Dictionary<string, string>(configVariables, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in cliVariables)
        {
            merged[key] = value;
        }

        return merged;
    }
}

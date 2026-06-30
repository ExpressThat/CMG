using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
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
        var configOption = new Option<FileInfo?>("--config") { Description = "JSON run config file. CLI options override config values." };
        var projectOption = new Option<string?>("--project") { Description = "Named project from the run config." };
        var gifOption = new Option<DirectoryInfo?>("--gif")
        {
            Description = "Write per-test GIF recordings to this directory."
        };
        gifOption.Aliases.Add("-gif");
        var gifQualityOption = new Option<string>("--gif-quality")
        {
            Description = "GIF quality for --gif recordings: highest, high, medium, or low.",
            DefaultValueFactory = _ => "highest"
        };
        var pointerDurationOption = new Option<int?>("--pointer-duration")
        {
            Description = "Default virtual pointer movement duration in milliseconds for --gif recordings."
        };
        var pointerSpeedOption = new Option<string?>("--pointer-speed")
        {
            Description = "Default virtual pointer speed for --gif recordings: slow, normal, fast, instant, or a multiplier like 1.5x."
        };
        var pointerEasingOption = new Option<string?>("--pointer-easing")
        {
            Description = "Default virtual pointer easing for --gif recordings: linear, ease-in, ease-out, ease-in-out, or spring."
        };
        var clickPulseOption = new Option<string?>("--click-pulse") { Description = "Click pulse style for --gif: ring, ripple, dot, crosshair, or none." };
        var holdAfterActionOption = new Option<int?>("--gif-hold-after-action") { Description = "Default post-action hold in milliseconds for --gif recordings." };
        var holdOnFailureOption = new Option<int?>("--gif-hold-on-failure") { Description = "Final failure-state hold in milliseconds for --gif recordings." };
        var jsonOption = new Option<FileInfo?>("--report-json") { Description = "Write a JSON test report to this file." };
        var htmlOption = new Option<FileInfo?>("--report-html") { Description = "Write an HTML test report to this file." };
        var junitOption = new Option<FileInfo?>("--report-junit") { Description = "Write a JUnit XML test report to this file." };
        var traceOption = new Option<DirectoryInfo?>("--trace") { Description = "Write per-test trace JSON files to this directory." };
        var grepOption = new Option<string?>("--grep") { Description = "Run tests whose names contain this text." };
        var tagOption = new Option<string?>("--tag") { Description = "Run tests with a matching tag option." };
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
        var browserPortOption = new Option<int?>("--browser-port")
        {
            Description = "Remote debugging port for the browser instance used by this run."
        };
        var autoLaunchOption = new Option<bool>("--auto-launch")
        {
            Description = "Launch the selected browser automatically when no CMG-controlled browser is running."
        };
        var headlessOption = new Option<bool>("--headless")
        {
            Description = "Launch the browser in headless mode when --auto-launch starts a browser."
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
            projectOption,
            gifOption,
            gifQualityOption,
            pointerDurationOption,
            pointerSpeedOption,
            pointerEasingOption,
            clickPulseOption,
            holdAfterActionOption,
            holdOnFailureOption,
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
            browserPortOption,
            autoLaunchOption,
            headlessOption,
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
            if (!TrySelectProject(parseResult.GetValue(projectOption), config, out var project, out var projectError))
            {
                Console.Error.WriteLine(projectError);
                return 1;
            }

            var variableValues = (parseResult.GetValue(variableOption) ?? [])
                .Concat(parseResult.GetValue(envOption) ?? []);
            if (!VariableOptionParser.TryParse(variableValues, out var variables, out var error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }
            if (!GifQualityParser.TryParse(parseResult.GetValue(gifQualityOption), out var gifQuality))
            {
                Console.Error.WriteLine($"--gif-quality must be one of: {GifQualityParser.Values}.");
                return 1;
            }
            if (!GifMotionOptionParser.TryParse(
                parseResult.GetValue(pointerDurationOption),
                parseResult.GetValue(pointerSpeedOption),
                parseResult.GetValue(pointerEasingOption),
                parseResult.GetValue(clickPulseOption),
                out var pointerMotion,
                out var clickPulse,
                out var motionError))
            {
                Console.Error.WriteLine(motionError);
                return 1;
            }
            if (!GifTimingOptionParser.TryParseHoldAfterAction(parseResult.GetValue(holdAfterActionOption), out var holdAfterAction, out var holdError))
            {
                Console.Error.WriteLine(holdError);
                return 1;
            }
            if (!GifTimingOptionParser.TryParseHoldOnFailure(parseResult.GetValue(holdOnFailureOption), out var holdOnFailure, out holdError))
            {
                Console.Error.WriteLine(holdError);
                return 1;
            }
            variables = MergeVariables(MergeVariables(config.Variables, project?.Variables), variables);
            var projectBrowser = BrowserKindFor(project?.Browser);
            if (projectBrowser is BrowserKind.InvalidSelection)
            {
                Console.Error.WriteLine($"Run config project '{project?.Name}' has unknown browser '{project?.Browser}'.");
                return 1;
            }

            return
            handler.Run(
                projectBrowser ?? CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(pathArgument) ?? string.Empty,
                DirectoryValue(parseResult, gifOption, config.Gif),
                FileValue(parseResult, jsonOption, config.ReportJson),
                FileValue(parseResult, htmlOption, config.ReportHtml),
                FileValue(parseResult, junitOption, config.ReportJunit),
                DirectoryValue(parseResult, traceOption, config.Trace),
                StringValue(parseResult, grepOption, project?.Grep ?? config.Grep),
                StringValue(parseResult, tagOption, project?.Tag ?? config.Tag),
                IntValue(parseResult, retriesOption, project?.Retries ?? config.Retries),
                IntValue(parseResult, maxFailuresOption, config.MaxFailures),
                IntValue(parseResult, repeatEachOption, config.RepeatEach),
                parseResult.GetValue(listOption),
                StringValue(parseResult, shardOption, config.Shard),
                IntValue(parseResult, timeoutOption, project?.Timeout ?? config.Timeout),
                IntValue(parseResult, navigationTimeoutOption, project?.NavigationTimeout ?? config.NavigationTimeout),
                IntValue(parseResult, assertionTimeoutOption, project?.AssertionTimeout ?? config.AssertionTimeout),
                StringValue(parseResult, baseUrlOption, project?.BaseUrl ?? config.BaseUrl),
                variables,
                project?.Name ?? string.Empty,
                parseResult.GetValue(browserPortOption),
                parseResult.GetValue(autoLaunchOption),
                parseResult.GetValue(headlessOption),
                gifQuality,
                pointerMotion,
                clickPulse,
                holdAfterAction,
                holdOnFailure);
        });

        return command;
    }

}

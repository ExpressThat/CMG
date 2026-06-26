using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildWorkersGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("workers", "Worker inspection, evaluation, and interception commands.");

        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "list", "List worker targets.", "listWorkers"));
        command.Subcommands.Add(BuildWaitForWorkerCommand(browserOptions));
        command.Subcommands.Add(BuildWorkerEvaluateCommand(browserOptions));
        command.Subcommands.Add(BuildWorkerInterceptCommand(browserOptions));

        return command;
    }

    private Command BuildCoverageGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("coverage", "JavaScript and CSS coverage commands.");

        command.Subcommands.Add(BuildStartCoverageCommand(browserOptions));
        command.Subcommands.Add(BuildStopCoverageCommand(browserOptions));

        return command;
    }

    private Command BuildWaitForWorkerCommand(BrowserSelectionOptions browserOptions)
    {
        var pattern = new Argument<string>("pattern") { Description = "Worker URL substring to match." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds. Default is 5000." };
        var command = new Command("wait", "Wait for a matching worker target.") { pattern, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForWorker", [parseResult.GetValue(pattern) ?? string.Empty], CompactOptions([
                IntOption("timeout", parseResult.GetValue(timeout))
            ]))));
        return command;
    }

    private Command BuildWorkerEvaluateCommand(BrowserSelectionOptions browserOptions)
    {
        var expression = new Argument<string>("expression") { Description = "JavaScript expression to evaluate in the worker." };
        var target = CliStringOption("--target", "Worker id or URL substring. Defaults to the first worker.");
        var command = new Command("evaluate", "Evaluate JavaScript in a worker target.") { expression, target };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("workerEvaluate", [parseResult.GetValue(expression) ?? string.Empty], CompactOptions([
                StringOption("target", parseResult.GetValue(target))
            ]))));
        return command;
    }

    private Command BuildWorkerInterceptCommand(BrowserSelectionOptions browserOptions)
    {
        var pattern = new Argument<string>("pattern") { Description = "Worker fetch URL substring to intercept." };
        var status = new Option<int?>("--status") { Description = "Mocked response status. Default is 200." };
        var body = CliStringOption("--body", "Mocked response body.");
        var contentType = CliStringOption("--content-type", "Mocked response content type. Default is text/plain.");
        var target = CliStringOption("--target", "Worker id or URL substring. Defaults to the first worker.");
        var command = new Command("intercept", "Patch worker fetch responses.") { pattern, status, body, contentType, target };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("workerIntercept", [parseResult.GetValue(pattern) ?? string.Empty], CompactOptions([
                IntOption("status", parseResult.GetValue(status)),
                StringOption("body", parseResult.GetValue(body)),
                StringOption("contentType", parseResult.GetValue(contentType)),
                StringOption("target", parseResult.GetValue(target))
            ]))));
        return command;
    }

    private Command BuildStartCoverageCommand(BrowserSelectionOptions browserOptions)
    {
        var js = CliStringOption("--js", "Collect JavaScript coverage: true or false. Default is true.");
        var css = CliStringOption("--css", "Collect CSS coverage: true or false. Default is true.");
        var command = new Command("start", "Start JavaScript and CSS coverage collection.") { js, css };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("startCoverage", [], CompactOptions([
                StringOption("js", parseResult.GetValue(js)),
                StringOption("css", parseResult.GetValue(css))
            ]))));
        return command;
    }

    private Command BuildStopCoverageCommand(BrowserSelectionOptions browserOptions)
    {
        var path = new Option<FileInfo?>("--path") { Description = "Write coverage JSON to this path instead of stdout." };
        var command = new Command("stop", "Stop coverage collection and print or write JSON.") { path };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("stopCoverage", [], CompactOptions([
                StringOption("path", parseResult.GetValue(path)?.FullName)
            ]))));
        return command;
    }
}

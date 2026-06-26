using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildWorkersGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("workers", "Worker inspection, evaluation, and interception commands.");

        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "list", "List worker targets.", "listWorkers"));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "listWorkers", "List worker targets.", "listWorkers"));
        command.Subcommands.Add(BuildWaitForWorkerCommand(browserOptions, "wait"));
        command.Subcommands.Add(BuildWaitForWorkerCommand(browserOptions, "waitForWorker"));
        command.Subcommands.Add(BuildWorkerEvaluateCommand(browserOptions, "evaluate"));
        command.Subcommands.Add(BuildWorkerEvaluateCommand(browserOptions, "workerEvaluate"));
        command.Subcommands.Add(BuildWorkerInterceptCommand(browserOptions, "intercept"));
        command.Subcommands.Add(BuildWorkerInterceptCommand(browserOptions, "workerIntercept"));

        return command;
    }

    private Command BuildCoverageGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("coverage", "JavaScript and CSS coverage commands.");

        command.Subcommands.Add(BuildStartCoverageCommand(browserOptions, "start"));
        command.Subcommands.Add(BuildStartCoverageCommand(browserOptions, "startCoverage"));
        command.Subcommands.Add(BuildStopCoverageCommand(browserOptions, "stop"));
        command.Subcommands.Add(BuildStopCoverageCommand(browserOptions, "stopCoverage"));

        return command;
    }

    private Command BuildWaitForWorkerCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var pattern = new Argument<string>("pattern") { Description = "Worker URL substring to match." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds. Default is 5000." };
        var command = new Command(name, "Wait for a matching worker target.") { pattern, timeout };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("waitForWorker", [parseResult.GetValue(pattern) ?? string.Empty], CompactOptions([
                IntOption("timeout", parseResult.GetValue(timeout))
            ]))));
        return command;
    }

    private Command BuildWorkerEvaluateCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var expression = new Argument<string>("expression") { Description = "JavaScript expression to evaluate in the worker." };
        var target = CliStringOption("--target", "Worker id or URL substring. Defaults to the first worker.");
        var command = new Command(name, "Evaluate JavaScript in a worker target.") { expression, target };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("workerEvaluate", [parseResult.GetValue(expression) ?? string.Empty], CompactOptions([
                StringOption("target", parseResult.GetValue(target))
            ]))));
        return command;
    }

    private Command BuildWorkerInterceptCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var pattern = new Argument<string>("pattern") { Description = "Worker fetch URL text to intercept." };
        var status = new Option<int?>("--status") { Description = "Mocked response status. Default is 200." };
        var body = CliStringOption("--body", "Mocked response body.");
        var bodyFile = new Option<FileInfo?>("--body-file") { Description = "Mocked response body file." };
        var contentType = CliStringOption("--content-type", "Mocked response content type. Default is text/plain.");
        var header = CliStringOption("--header", "Response header as Name: value.");
        var headers = CliStringOption("--headers", "Response headers separated by semicolons.");
        var headerName = CliStringOption("--header-name", "Response header name.");
        var headerValue = CliStringOption("--header-value", "Response header value.");
        var match = NavigationMatchOption();
        var ignoreCase = NavigationIgnoreCaseOption();
        var target = CliStringOption("--target", "Worker id or URL substring. Defaults to the first worker.");
        var command = new Command(name, "Patch worker fetch responses.")
        {
            pattern,
            status,
            body,
            bodyFile,
            contentType,
            header,
            headers,
            headerName,
            headerValue,
            match,
            ignoreCase,
            target
        };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("workerIntercept", [parseResult.GetValue(pattern) ?? string.Empty], CompactOptions([
                IntOption("status", parseResult.GetValue(status)),
                StringOption("body", parseResult.GetValue(body)),
                StringOption("bodyFile", parseResult.GetValue(bodyFile)?.FullName),
                StringOption("contentType", parseResult.GetValue(contentType)),
                StringOption("header", parseResult.GetValue(header)),
                StringOption("headers", parseResult.GetValue(headers)),
                StringOption("headerName", parseResult.GetValue(headerName)),
                StringOption("headerValue", parseResult.GetValue(headerValue)),
                StringOption("match", parseResult.GetValue(match)),
                parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null,
                StringOption("target", parseResult.GetValue(target))
            ]))));
        return command;
    }

    private Command BuildStartCoverageCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var js = CliStringOption("--js", "Collect JavaScript coverage: true or false. Default is true.");
        var css = CliStringOption("--css", "Collect CSS coverage: true or false. Default is true.");
        var command = new Command(name, "Start JavaScript and CSS coverage collection.") { js, css };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("startCoverage", [], CompactOptions([
                StringOption("js", parseResult.GetValue(js)),
                StringOption("css", parseResult.GetValue(css))
            ]))));
        return command;
    }

    private Command BuildStopCoverageCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var path = new Option<FileInfo?>("--path") { Description = "Write coverage JSON to this path instead of stdout." };
        var command = new Command(name, "Stop coverage collection and print or write JSON.") { path };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("stopCoverage", [], CompactOptions([
                StringOption("path", parseResult.GetValue(path)?.FullName)
            ]))));
        return command;
    }
}

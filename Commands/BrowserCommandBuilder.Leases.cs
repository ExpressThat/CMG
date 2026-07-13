using System.CommandLine;

namespace CMG.Commands;

public sealed partial class BrowserCommandBuilder
{
    private Command BuildLeaseCommand(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("lease", "Inspect or manage conservative headless-browser idle cleanup.");
        var status = new Command("status", "Show the selected browser's idle lease.");
        status.SetAction(parseResult => browserCommandHandler.LeaseStatus(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions)));

        var timeout = new Option<string?>("--idle-timeout")
        {
            Description = "Optional replacement lease duration, such as 30m or 2h."
        };
        var keepAlive = new Command("keepAlive", "Renew the selected browser's idle lease.") { timeout };
        keepAlive.SetAction(parseResult =>
        {
            if (!BrowserIdleTimeoutParser.TryParse(parseResult.GetValue(timeout), out var milliseconds, out var error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }
            return browserCommandHandler.LeaseKeepAlive(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
                milliseconds);
        });

        var disable = new Command("disable", "Disable idle cleanup without closing the browser.");
        disable.SetAction(parseResult => browserCommandHandler.LeaseDisable(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions)));

        var token = new Option<string>("--token") { Required = true };
        var monitor = new Command("monitor", "Run the internal lease monitor.") { Hidden = true };
        monitor.Options.Add(token);
        monitor.SetAction(parseResult => browserCommandHandler.LeaseMonitor(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions),
            parseResult.GetValue(token)!));

        command.Subcommands.Add(status);
        command.Subcommands.Add(keepAlive);
        command.Subcommands.Add(disable);
        command.Subcommands.Add(monitor);
        return command;
    }
}

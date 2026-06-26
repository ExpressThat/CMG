using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildClockGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("clock", "Deterministic page-side time commands.");

        command.Subcommands.Add(BuildClockInstallCommand(browserOptions));
        command.Subcommands.Add(BuildClockTickCommand(browserOptions));
        command.Subcommands.Add(BuildNetworkNoArgumentCommand(browserOptions, "restore", "Restore native page clock APIs.", "restoreClock"));

        return command;
    }

    private Command BuildAccessibilityGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("accessibility", "Accessibility snapshot and assertion commands.");

        command.Subcommands.Add(BuildAccessibilitySnapshotCommand(browserOptions));
        command.Subcommands.Add(BuildExpectAccessibleCommand(browserOptions));

        return command;
    }

    private Command BuildClockInstallCommand(BrowserSelectionOptions browserOptions)
    {
        var now = new Option<long?>("--now") { Description = "Epoch milliseconds. Default is current host time." };
        var command = new Command("install", "Install deterministic page-side time control.") { now };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("clock", [], CompactOptions([
                StringOption("now", parseResult.GetValue(now)?.ToString())
            ]))));

        return command;
    }

    private Command BuildClockTickCommand(BrowserSelectionOptions browserOptions)
    {
        var milliseconds = new Argument<long>("milliseconds") { Description = "Non-negative milliseconds to advance." };
        var command = new Command("tick", "Advance deterministic page-side time.") { milliseconds };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("tick", parseResult.GetValue(milliseconds).ToString())));

        return command;
    }

    private Command BuildAccessibilitySnapshotCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = OptionalTextArgument("selector", "Optional CSS selector to snapshot.");
        var output = new Option<FileInfo?>("--output") { Description = "Write snapshot JSON to this file." };
        var command = new Command("snapshot", "Create an accessibility snapshot.") { selector, output };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("accessibilitySnapshot", OptionalArgument(parseResult, selector), CompactOptions([
                StringOption("output", parseResult.GetValue(output)?.FullName)
            ]))));

        return command;
    }

    private Command BuildExpectAccessibleCommand(BrowserSelectionOptions browserOptions)
    {
        var role = new Option<string>("--role")
        {
            Description = "Required accessible role.",
            Required = true
        };
        var name = CliStringOption("--name", "Optional text expected in the accessible name.");
        var command = new Command("expect", "Assert that an accessible element exists.") { role, name };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("expectAccessible", [], CompactOptions([
                StringOption("role", parseResult.GetValue(role)),
                StringOption("name", parseResult.GetValue(name))
            ]))));

        return command;
    }
}

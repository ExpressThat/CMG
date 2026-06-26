using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildTabsGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("tabs", "Tab and popup target commands.");

        command.Subcommands.Add(BuildNoArgumentCommand(browserOptions, "list", "List available page targets.", "listTabs"));
        command.Subcommands.Add(BuildOpenTabCommand(browserOptions));
        command.Subcommands.Add(BuildWaitForTabCommand(browserOptions));
        command.Subcommands.Add(BuildIndexedCommand(browserOptions, "activate", "Activate a page target by index.", "activateTab"));
        command.Subcommands.Add(BuildIndexedCommand(browserOptions, "close", "Close a page target by index.", "closeTab"));

        return command;
    }

    private Command BuildNoArgumentCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        return BuildNoArgumentCommand(browserOptions, name, description, name);
    }

    private Command BuildNoArgumentCommand(BrowserSelectionOptions browserOptions, string name, string description, string action)
    {
        var command = new Command(name, description);

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), action));

        return command;
    }

    private Command BuildIndexedCommand(BrowserSelectionOptions browserOptions, string name, string description)
    {
        return BuildIndexedCommand(browserOptions, name, description, name);
    }

    private Command BuildIndexedCommand(BrowserSelectionOptions browserOptions, string name, string description, string action)
    {
        var indexOption = new Option<int>("--index")
        {
            Description = "Zero-based tab index.",
            Required = true
        };

        var command = new Command(name, description)
        {
            indexOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(action, [], [("index", parseResult.GetValue(indexOption).ToString())])));

        return command;
    }

    private Command BuildOpenTabCommand(BrowserSelectionOptions browserOptions)
    {
        var targetArgument = new Argument<string>("target")
        {
            Description = "URL, data URL, or local file path to open in a new tab."
        };

        var command = new Command("open", "Open a new tab.")
        {
            targetArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine("openTab", parseResult.GetValue(targetArgument) ?? string.Empty)));

        return command;
    }

    private Command BuildWaitForTabCommand(BrowserSelectionOptions browserOptions)
    {
        var countOption = new Option<int>("--count")
        {
            Description = "Minimum tab count to wait for.",
            Required = true
        };
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Timeout in milliseconds.",
            DefaultValueFactory = _ => 5000
        };

        var command = new Command("wait", "Wait until at least this many tabs exist.")
        {
            countOption,
            timeoutOption
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "waitForTab",
                [],
                [
                    ("count", parseResult.GetValue(countOption).ToString()),
                    ("timeout", parseResult.GetValue(timeoutOption).ToString())
                ])));

        return command;
    }

    private Command BuildSetCommand(BrowserSelectionOptions browserOptions)
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "Variable name."
        };
        var valueArgument = new Argument<string>("value")
        {
            Description = "Variable value."
        };

        var command = new Command("set", "Set a script variable for this one action invocation.")
        {
            nameArgument,
            valueArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "set",
                parseResult.GetValue(nameArgument) ?? string.Empty,
                parseResult.GetValue(valueArgument) ?? string.Empty)));

        return command;
    }

    private static Argument<string> CreateSelectorArgument()
    {
        return new Argument<string>("selector")
        {
            Description = "CSS selector."
        };
    }

    private static IReadOnlyList<(string Key, string Value)> ToOutputOptions(FileInfo? output)
    {
        return output is null ? [] : [("output", output.FullName)];
    }

    private static string ToScriptLine(string action, string argument)
    {
        return ToScriptLine(action, [argument], []);
    }

    private static string ToScriptLine(string action, string firstArgument, string secondArgument)
    {
        return ToScriptLine(action, [firstArgument, secondArgument], []);
    }

    private static string ToScriptLine(
        string action,
        IReadOnlyList<string> arguments,
        IReadOnlyList<(string Key, string Value)> options)
    {
        var parts = new List<string> { action };
        parts.AddRange(arguments.Select(QuoteScriptValue));
        parts.AddRange(options.Select(option => $"{option.Key}={QuoteScriptValue(option.Value)}"));

        return string.Join(' ', parts);
    }

    private static string QuoteScriptValue(string value)
    {
        return $"\"{value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)}\"";
    }
}

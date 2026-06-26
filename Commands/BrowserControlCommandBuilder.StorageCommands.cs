using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildStorageGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("storage", "Storage and persisted browser state commands.");

        command.Subcommands.Add(BuildStorageAreaCommand(browserOptions, "local", "localStorage", "Read or mutate localStorage.", "Storage key."));
        command.Subcommands.Add(BuildStorageAreaCommand(browserOptions, "session", "sessionStorage", "Read or mutate sessionStorage.", "Storage key."));
        command.Subcommands.Add(BuildStorageAreaCommand(browserOptions, "cookie", "cookie", "Read or mutate document cookies.", "Cookie name. Optional for get."));
        command.Subcommands.Add(BuildStorageStateCommand(browserOptions));

        return command;
    }

    private Command BuildStorageAreaCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action,
        string description,
        string keyDescription)
    {
        var operationArgument = new Argument<string>("operation")
        {
            Description = "Operation: get, set, remove, or clear."
        };
        var keyArgument = new Argument<string?>("key")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = keyDescription
        };
        var valueArgument = new Argument<string?>("value")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Value for set operations."
        };

        var command = new Command(name, description)
        {
            operationArgument,
            keyArgument,
            valueArgument
        };

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                action,
                Compact([
                    parseResult.GetValue(operationArgument),
                    parseResult.GetValue(keyArgument),
                    parseResult.GetValue(valueArgument)
                ]),
                [])));

        return command;
    }

    private Command BuildStorageStateCommand(BrowserSelectionOptions browserOptions)
    {
        var operationArgument = new Argument<string>("operation")
        {
            Description = "Operation: save or load."
        };
        var pathOption = new Option<FileInfo?>("--path")
        {
            Description = "Storage state JSON file path. Defaults to cmg-storage-state.json."
        };

        var command = new Command("state", "Save or load localStorage, sessionStorage, and cookies.")
        {
            operationArgument,
            pathOption
        };

        command.SetAction(parseResult =>
        {
            var options = ToOutputOptions(parseResult.GetValue(pathOption));
            return browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), ToScriptLine(
                "storageState",
                [parseResult.GetValue(operationArgument) ?? string.Empty],
                options.Select(option => (option.Key is "output" ? "path" : option.Key, option.Value)).ToArray()));
        });

        return command;
    }

    private static IReadOnlyList<string> Compact(IReadOnlyList<string?> values) =>
        values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToArray();
}

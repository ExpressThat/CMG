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
        command.Subcommands.Add(BuildStorageStateCommand(browserOptions, "state"));
        command.Subcommands.Add(BuildStorageStateCommand(browserOptions, "storageState"));

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

        var domain = new Option<string?>("--domain") { Description = "Cookie domain for set, remove, or clear." };
        var path = new Option<string?>("--path") { Description = "Cookie path. Defaults to /." };
        var expires = new Option<string?>("--expires") { Description = "Cookie expiry date string for set." };
        var maxAge = new Option<int?>("--max-age") { Description = "Cookie Max-Age in seconds for set." };
        var sameSite = new Option<string?>("--same-site") { Description = "Cookie SameSite value: Strict, Lax, or None." };
        var secure = new Option<bool>("--secure") { Description = "Set the Secure cookie attribute." };
        if (action is "cookie")
        {
            command.Options.Add(domain);
            command.Options.Add(path);
            command.Options.Add(expires);
            command.Options.Add(maxAge);
            command.Options.Add(sameSite);
            command.Options.Add(secure);
        }

        command.SetAction(parseResult =>
            browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
                action,
                Compact([
                    parseResult.GetValue(operationArgument),
                    parseResult.GetValue(keyArgument),
                    parseResult.GetValue(valueArgument)
                ]),
                action is "cookie"
                    ? CookieOptions(parseResult, domain, path, expires, maxAge, sameSite, secure)
                    : [])));

        return command;
    }

    private Command BuildStorageStateCommand(BrowserSelectionOptions browserOptions, string name)
    {
        var operationArgument = new Argument<string>("operation")
        {
            Description = "Operation: save or load."
        };
        var pathOption = new Option<FileInfo?>("--path")
        {
            Description = "Storage state JSON file path. Defaults to cmg-storage-state.json."
        };

        var command = new Command(name, "Save or load localStorage, sessionStorage, and cookies.")
        {
            operationArgument,
            pathOption
        };

        command.SetAction(parseResult =>
        {
            var options = ToOutputOptions(parseResult.GetValue(pathOption));
            return browserControlCommandHandler.RunScriptAction(CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions), CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions), ToScriptLine(
                "storageState",
                [parseResult.GetValue(operationArgument) ?? string.Empty],
                options.Select(option => (option.Key is "output" ? "path" : option.Key, option.Value)).ToArray()));
        });

        return command;
    }

    private static IReadOnlyList<string> Compact(IReadOnlyList<string?> values) =>
        values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToArray();

    private static IReadOnlyList<(string Key, string Value)> CookieOptions(
        ParseResult parseResult,
        Option<string?> domain,
        Option<string?> path,
        Option<string?> expires,
        Option<int?> maxAge,
        Option<string?> sameSite,
        Option<bool> secure)
    {
        var options = CompactOptions([
            StringOption("domain", parseResult.GetValue(domain)),
            StringOption("path", parseResult.GetValue(path)),
            StringOption("expires", parseResult.GetValue(expires)),
            IntOption("maxAge", parseResult.GetValue(maxAge)),
            StringOption("sameSite", parseResult.GetValue(sameSite))
        ]).ToList();
        if (parseResult.GetValue(secure))
        {
            options.Add(("secure", "true"));
        }

        return options;
    }
}

using System.CommandLine;
using CMG.Runner;

namespace CMG.Commands;

public sealed class ApiCommandBuilder
{
    private readonly CmgApiRequestRunner runner;

    public ApiCommandBuilder(CmgApiRequestRunner runner)
    {
        this.runner = runner;
    }

    public Command Build()
    {
        var command = new Command("api", "HTTP API utility commands.");
        command.Subcommands.Add(BuildRequestCommand());
        return command;
    }

    private Command BuildRequestCommand()
    {
        var method = new Argument<string>("method") { Description = "HTTP method." };
        var url = new Argument<string>("url") { Description = "Absolute request URL." };
        var body = new Option<string?>("--body") { Description = "Raw request body." };
        var json = new Option<string?>("--json") { Description = "JSON request body." };
        var contentType = new Option<string?>("--content-type") { Description = "Request content type." };
        var timeout = new Option<int?>("--timeout") { Description = "Timeout in milliseconds. Default is 30000." };
        var status = new Option<int?>("--status") { Description = "Expected response status." };
        var contains = new Option<string?>("--contains") { Description = "Expected response body text." };
        var headers = PairOption("--header", "Header as Name=Value. Repeatable.");
        var queries = PairOption("--query", "Query parameter as Name=Value. Repeatable.");
        var command = new Command("request", "Send an HTTP request.") { method, url, body, json, contentType, timeout, status, contains, headers, queries };

        command.SetAction(parseResult =>
        {
            var options = Options(parseResult, body, json, contentType, timeout, status, contains);
            AddPairs(options, "header.", parseResult.GetValue(headers) ?? []);
            AddPairs(options, "query.", parseResult.GetValue(queries) ?? []);
            var step = runner.Run(new CmgNode(1, "apiRequest", "apiRequest", [parseResult.GetValue(method) ?? string.Empty, parseResult.GetValue(url) ?? string.Empty], options, []));
            foreach (var line in step.Output)
            {
                Console.WriteLine(line);
            }

            if (step.Success)
            {
                return 0;
            }

            Console.Error.WriteLine(step.Error);
            return 1;
        });

        return command;
    }

    private static Option<string[]> PairOption(string name, string description) =>
        new(name)
        {
            Arity = ArgumentArity.ZeroOrMore,
            Description = description
        };

    private static Dictionary<string, string> Options(
        ParseResult parseResult,
        Option<string?> body,
        Option<string?> json,
        Option<string?> contentType,
        Option<int?> timeout,
        Option<int?> status,
        Option<string?> contains)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(options, "body", parseResult.GetValue(body));
        Add(options, "json", parseResult.GetValue(json));
        Add(options, "contentType", parseResult.GetValue(contentType));
        Add(options, "timeout", parseResult.GetValue(timeout)?.ToString());
        Add(options, "status", parseResult.GetValue(status)?.ToString());
        Add(options, "contains", parseResult.GetValue(contains));
        return options;
    }

    private static void Add(Dictionary<string, string> options, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            options[key] = value;
        }
    }

    private static void AddPairs(Dictionary<string, string> options, string prefix, IReadOnlyList<string> pairs)
    {
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length is 2 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                options[$"{prefix}{parts[0]}"] = parts[1];
            }
        }
    }
}

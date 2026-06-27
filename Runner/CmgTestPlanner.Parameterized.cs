using System.Text.Json;
using System.Text.RegularExpressions;

namespace CMG.Runner;

public sealed partial class CmgTestPlanner
{
    private static IEnumerable<CmgNode> ExpandParameterizedTests(CmgNode test)
    {
        if (!IsParameterized(test))
        {
            yield return test;
            yield break;
        }

        var variable = test.Options.GetValueOrDefault("as") ?? "item";
        var rows = ParameterRows(test, variable).ToArray();
        for (var index = 0; index < rows.Length; index++)
        {
            var variables = rows[index].Variables.Prepend(("index", index.ToString())).ToArray();
            var name = ExpandName(test.Name, variables);
            if (name.Equals(test.Name, StringComparison.Ordinal))
            {
                name = $"{test.Name} [{variable}={rows[index].Display}]";
            }

            yield return test with
            {
                Name = name,
                Options = CleanParameterizedOptions(test.Options),
                Children = SetNodes(test.LineNumber, variables).Concat(test.Children).ToArray()
            };
        }
    }

    private static bool IsParameterized(CmgNode test) =>
        test.Options.ContainsKey("json") ||
        test.Options.ContainsKey("values") ||
        test.Options.ContainsKey("each");

    private static IEnumerable<ParameterRow> ParameterRows(CmgNode test, string variable)
    {
        if (test.Options.TryGetValue("json", out var json))
        {
            return JsonRows(json, variable);
        }

        var values = test.Options.TryGetValue("values", out var listed)
            ? listed
            : test.Options.GetValueOrDefault("each") ?? string.Empty;
        var delimiter = test.Options.GetValueOrDefault("delimiter") ?? ",";
        return values.Split(delimiter, StringSplitOptions.TrimEntries)
            .Where(value => value.Length > 0)
            .Select(value => new ParameterRow(value, [(variable, value)]));
    }

    private static IEnumerable<ParameterRow> JsonRows(string json, string variable)
    {
        using var document = JsonDocument.Parse(json);
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.ValueKind is JsonValueKind.Object)
            {
                var raw = item.GetRawText();
                var variables = new List<(string, string)> { (variable, raw) };
                variables.AddRange(item.EnumerateObject().Select(property => ($"{variable}.{property.Name}", JsonValue(property.Value))));
                yield return new ParameterRow(raw, variables);
            }
            else
            {
                var value = JsonValue(item);
                yield return new ParameterRow(value, [(variable, value)]);
            }
        }
    }

    private static IReadOnlyDictionary<string, string> CleanParameterizedOptions(IReadOnlyDictionary<string, string> options) =>
        options.Where(option => !ParameterizedOptionNames.Contains(option.Key))
            .ToDictionary(option => option.Key, option => option.Value, StringComparer.OrdinalIgnoreCase);

    private static IEnumerable<CmgNode> SetNodes(int lineNumber, IReadOnlyList<(string Key, string Value)> variables) =>
        variables.Select(pair => new CmgNode(lineNumber, "set", "set", [pair.Key, pair.Value], new Dictionary<string, string>(), []));

    private static string ExpandName(string value, IReadOnlyList<(string Key, string Value)> variables) =>
        ParameterNameRegex().Replace(value, match => variables.FirstOrDefault(pair => pair.Key == match.Groups[1].Value).Value ?? match.Value);

    private static string JsonValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.ToString(),
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };

    private static readonly HashSet<string> ParameterizedOptionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "as", "each", "values", "json", "delimiter"
    };

    [GeneratedRegex(@"\$\{([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\}")]
    private static partial Regex ParameterNameRegex();

    private sealed record ParameterRow(string Display, IReadOnlyList<(string Key, string Value)> Variables);
}

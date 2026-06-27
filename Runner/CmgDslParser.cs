using System.Text.Json;
using System.Text.RegularExpressions;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgDslParser
{
    public CmgParseResult Parse(string sourcePath, string script)
    {
        var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(sourcePath)) ?? Directory.GetCurrentDirectory();
        var importResult = ScriptImportExpander.Expand(script, baseDirectory);
        if (!importResult.Success)
        {
            return CmgParseResult.Fail(importResult.Error ?? "Could not import script.");
        }

        script = importResult.Script ?? string.Empty;
        var lines = ScriptLineNormalizer.Normalize(script);
        var result = ParseNodes(lines, 0, stopAtBlockEnd: false);
        if (!result.Success)
        {
            return CmgParseResult.Fail(result.Error ?? "Could not parse script.");
        }

        var validation = ValidateTopLevel(result.Nodes);
        return validation is null
            ? CmgParseResult.Ok(new CmgDocument(sourcePath, result.Nodes))
            : CmgParseResult.Fail(validation);
    }

    private static CmgNodeListResult ParseNodes(string[] lines, int startIndex, bool stopAtBlockEnd)
    {
        var nodes = new List<CmgNode>();
        for (var index = startIndex; index < lines.Length; index++)
        {
            var rawLine = lines[index];
            var lineNumber = index + 1;
            var trimmed = rawLine.Trim();
            if (trimmed.Length is 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            if (trimmed is "}")
            {
                return stopAtBlockEnd
                    ? CmgNodeListResult.Ok(nodes, index)
                    : CmgNodeListResult.Fail($"Line {lineNumber}: unexpected block close '}}'.");
            }

            var hasBlock = trimmed.EndsWith('{');
            if (hasBlock)
            {
                trimmed = trimmed[..^1].TrimEnd();
            }

            var tokens = Tokenize(trimmed, lineNumber);
            if (!tokens.Success)
            {
                return CmgNodeListResult.Fail(tokens.Error ?? $"Line {lineNumber}: invalid syntax.");
            }

            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var args = new List<string>();
            foreach (var token in tokens.Tokens.Skip(1))
            {
                var equals = token.IndexOf('=');
                if (equals > 0 && IsOptionKey(token[..equals]))
                {
                    options[token[..equals]] = token[(equals + 1)..];
                }
                else
                {
                    args.Add(token);
                }
            }

            IReadOnlyList<CmgNode> children = [];
            if (hasBlock)
            {
                var childResult = ParseNodes(lines, index + 1, stopAtBlockEnd: true);
                if (!childResult.Success)
                {
                    return childResult;
                }

                children = childResult.Nodes;
                index = childResult.NextIndex;
            }

            var declaration = CmgDeclarationAliases.Normalize(tokens.Tokens[0], options);
            nodes.Add(new CmgNode(lineNumber, declaration.Kind, args.FirstOrDefault() ?? declaration.Kind, args, declaration.Options, children));
        }

        return stopAtBlockEnd
            ? CmgNodeListResult.Fail($"Line {lines.Length}: missing block close '}}'.")
            : CmgNodeListResult.Ok(nodes, lines.Length);
    }

    private static CmgTokenizeResult Tokenize(string line, int lineNumber)
    {
        var tokens = new List<string>();
        var current = new List<char>();
        var inQuotes = false;
        var tokenStarted = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character is '\\' && inQuotes && index + 1 < line.Length)
            {
                current.Add(line[++index] switch { 'n' => '\n', 'r' => '\r', 't' => '\t', var next => next });
                continue;
            }

            if (character is '"')
            {
                inQuotes = !inQuotes;
                tokenStarted = true;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddToken(tokens, current, ref tokenStarted);
                continue;
            }

            tokenStarted = true;
            current.Add(character);
        }

        if (inQuotes)
        {
            return CmgTokenizeResult.Fail($"Line {lineNumber}: unterminated quoted string.");
        }

        AddToken(tokens, current, ref tokenStarted);
        return tokens.Count is 0
            ? CmgTokenizeResult.Fail($"Line {lineNumber}: expected a command.")
            : CmgTokenizeResult.Ok(tokens);
    }

    private static void AddToken(List<string> tokens, List<char> current, ref bool tokenStarted)
    {
        if (!tokenStarted)
        {
            return;
        }

        tokens.Add(new string(current.ToArray()));
        current.Clear();
        tokenStarted = false;
    }

    private static bool IsOptionKey(string value) =>
        value.Length > 0 &&
        char.IsLetter(value[0]) &&
        value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.');

    private static string? ValidateTopLevel(IReadOnlyList<CmgNode> nodes)
    {
        var invalid = nodes.FirstOrDefault(node =>
            !IsAny(node, "suite", "describe", "context") &&
            !IsAny(node, "test", "it", "specify") &&
            !IsAny(node, "beforeAll", "before") &&
            !IsAny(node, "afterAll", "after") &&
            !node.Kind.Equals("beforeEach", StringComparison.OrdinalIgnoreCase) &&
            !node.Kind.Equals("afterEach", StringComparison.OrdinalIgnoreCase) &&
            !node.Kind.Equals("macro", StringComparison.OrdinalIgnoreCase));

        if (invalid is not null)
        {
            return $"Line {invalid.LineNumber}: cmg run requires structured tests with test/it/specify or suite/describe/context blocks. Direct browser-control scripts should use browser control script --file; see docs/scripting/migration.md.";
        }

        return ValidateParameterizedTests(nodes);
    }

    private static bool IsAny(CmgNode node, params string[] names) =>
        names.Any(name => node.Kind.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static string? ValidateParameterizedTests(IEnumerable<CmgNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (IsAny(node, "test", "it", "specify") && IsParameterized(node))
            {
                var error = ValidateParameterizedTest(node);
                if (error is not null) return error;
            }

            var childError = ValidateParameterizedTests(node.Children);
            if (childError is not null) return childError;
        }

        return null;
    }

    private static bool IsParameterized(CmgNode node) =>
        node.Options.ContainsKey("json") ||
        node.Options.ContainsKey("values") ||
        node.Options.ContainsKey("each");

    private static string? ValidateParameterizedTest(CmgNode node)
    {
        var variable = node.Options.GetValueOrDefault("as") ?? "item";
        if (!ParameterVariableRegex().IsMatch(variable))
        {
            return $"Line {node.LineNumber}: parameter variable name '{variable}' is invalid.";
        }

        if (node.Options.TryGetValue("json", out var json))
        {
            return ValidateJsonRows(node.LineNumber, json);
        }

        var values = node.Options.TryGetValue("values", out var listed)
            ? listed
            : node.Options.GetValueOrDefault("each") ?? string.Empty;
        if (values.Equals("true", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(values))
        {
            return $"Line {node.LineNumber}: parameterized test requires values=, each=, or json= data.";
        }

        return null;
    }

    private static string? ValidateJsonRows(int lineNumber, string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind is JsonValueKind.Array
                ? null
                : $"Line {lineNumber}: parameterized test json= must be a JSON array.";
        }
        catch (JsonException exception)
        {
            return $"Line {lineNumber}: parameterized test json= could not be parsed: {exception.Message}";
        }
    }

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex ParameterVariableRegex();
}

namespace CMG.Runner;

public sealed class CmgDslParser
{
    public CmgParseResult Parse(string sourcePath, string script)
    {
        var lines = script.ReplaceLineEndings("\n").Split('\n');
        var result = ParseNodes(lines, 0, stopAtBlockEnd: false);
        return result.Success
            ? CmgParseResult.Ok(new CmgDocument(sourcePath, result.Nodes))
            : CmgParseResult.Fail(result.Error ?? "Could not parse script.");
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

            nodes.Add(new CmgNode(lineNumber, tokens.Tokens[0], args.FirstOrDefault() ?? tokens.Tokens[0], args, options, children));
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
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddToken(tokens, current);
                continue;
            }

            current.Add(character);
        }

        if (inQuotes)
        {
            return CmgTokenizeResult.Fail($"Line {lineNumber}: unterminated quoted string.");
        }

        AddToken(tokens, current);
        return tokens.Count is 0
            ? CmgTokenizeResult.Fail($"Line {lineNumber}: expected a command.")
            : CmgTokenizeResult.Ok(tokens);
    }

    private static void AddToken(List<string> tokens, List<char> current)
    {
        if (current.Count is 0)
        {
            return;
        }

        tokens.Add(new string(current.ToArray()));
        current.Clear();
    }

    private static bool IsOptionKey(string value) =>
        value.Length > 0 &&
        char.IsLetter(value[0]) &&
        value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_');
}

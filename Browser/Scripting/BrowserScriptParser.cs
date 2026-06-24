namespace CMG.Browser.Scripting;

public sealed class BrowserScriptParser
{
    public ScriptParseResult Parse(string script)
    {
        var lines = script.ReplaceLineEndings("\n").Split('\n');
        var parseResult = ParseActions(lines, 0, stopAtBlockEnd: false);
        if (!parseResult.Success)
        {
            return ScriptParseResult.Fail(parseResult.Error ?? "Could not parse script.");
        }

        return ScriptParseResult.Ok(parseResult.Actions);
    }

    private static ActionListParseResult ParseActions(string[] lines, int startIndex, bool stopAtBlockEnd)
    {
        var actions = new List<BrowserScriptAction>();

        for (var index = startIndex; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var rawLine = lines[index];
            var trimmed = rawLine.Trim();

            if (trimmed.Length is 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            if (trimmed is "}")
            {
                if (!stopAtBlockEnd)
                {
                    return ActionListParseResult.Fail($"Line {lineNumber}: unexpected block close '}}'.");
                }

                return ActionListParseResult.Ok(actions, index);
            }

            var hasBlock = trimmed.EndsWith('{');
            if (trimmed.Contains('}', StringComparison.Ordinal))
            {
                return ActionListParseResult.Fail($"Line {lineNumber}: unexpected block close '}}'.");
            }

            if (hasBlock)
            {
                trimmed = trimmed[..^1].TrimEnd();
                if (trimmed.Length is 0)
                {
                    return ActionListParseResult.Fail($"Line {lineNumber}: block opener '{{' must follow an action.");
                }
            }

            var tokenResult = Tokenize(trimmed, lineNumber);
            if (!tokenResult.Success)
            {
                return ActionListParseResult.Fail(tokenResult.Error ?? $"Line {lineNumber}: invalid syntax.");
            }

            var tokens = tokenResult.Tokens;
            if (tokens.Count is 0)
            {
                continue;
            }

            var positional = new List<string>();
            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var token in tokens.Skip(1))
            {
                var equalsIndex = token.IndexOf('=');
                if (equalsIndex > 0 && IsOptionKey(token[..equalsIndex]))
                {
                    options[token[..equalsIndex]] = token[(equalsIndex + 1)..];
                    continue;
                }

                positional.Add(token);
            }

            IReadOnlyList<BrowserScriptAction> children = [];
            if (hasBlock)
            {
                var childResult = ParseActions(lines, index + 1, stopAtBlockEnd: true);
                if (!childResult.Success)
                {
                    return childResult;
                }

                children = childResult.Actions;
                index = childResult.NextIndex;
            }

            actions.Add(new BrowserScriptAction(
                lineNumber,
                rawLine,
                tokens[0],
                positional,
                options,
                children));
        }

        if (stopAtBlockEnd)
        {
            return ActionListParseResult.Fail($"Line {lines.Length}: missing block close '}}'.");
        }

        return ActionListParseResult.Ok(actions, lines.Length);
    }

    private static TokenizeResult Tokenize(string line, int lineNumber)
    {
        var tokens = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];

            if (character is '\\' && inQuotes)
            {
                if (index + 1 < line.Length && line[index + 1] is '"' or '\\')
                {
                    current.Add(line[index + 1]);
                    index++;
                    continue;
                }

                if (index + 1 < line.Length && line[index + 1] is 'n' or 'r' or 't')
                {
                    current.Add(line[index + 1] switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        _ => '\t'
                    });
                    index++;
                    continue;
                }

                current.Add(character);
                continue;
            }

            if (character is '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddCurrentToken(tokens, current);
                continue;
            }

            current.Add(character);
        }

        if (inQuotes)
        {
            return TokenizeResult.Fail($"Line {lineNumber}: unterminated quoted string.");
        }

        AddCurrentToken(tokens, current);

        return TokenizeResult.Ok(tokens);
    }

    private static void AddCurrentToken(List<string> tokens, List<char> current)
    {
        if (current.Count is 0)
        {
            return;
        }

        tokens.Add(new string(current.ToArray()));
        current.Clear();
    }

    private static bool IsOptionKey(string value)
    {
        if (value.Length is 0 || !char.IsLetter(value[0]))
        {
            return false;
        }

        return value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_');
    }
}

public sealed record BrowserScriptAction(
    int LineNumber,
    string RawLine,
    string Name,
    IReadOnlyList<string> Arguments,
    IReadOnlyDictionary<string, string> Options,
    IReadOnlyList<BrowserScriptAction> Children);

public sealed record ScriptParseResult(bool Success, IReadOnlyList<BrowserScriptAction> Actions, string? Error)
{
    public static ScriptParseResult Ok(IReadOnlyList<BrowserScriptAction> actions) => new(true, actions, null);

    public static ScriptParseResult Fail(string error) => new(false, [], error);
}

internal sealed record TokenizeResult(bool Success, IReadOnlyList<string> Tokens, string? Error)
{
    public static TokenizeResult Ok(IReadOnlyList<string> tokens) => new(true, tokens, null);

    public static TokenizeResult Fail(string error) => new(false, [], error);
}

internal sealed record ActionListParseResult(
    bool Success,
    IReadOnlyList<BrowserScriptAction> Actions,
    int NextIndex,
    string? Error)
{
    public static ActionListParseResult Ok(IReadOnlyList<BrowserScriptAction> actions, int nextIndex) =>
        new(true, actions, nextIndex, null);

    public static ActionListParseResult Fail(string error) => new(false, [], 0, error);
}

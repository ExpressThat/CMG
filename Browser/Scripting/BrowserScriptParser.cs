namespace CMG.Browser.Scripting;

public sealed class BrowserScriptParser
{
    public ScriptParseResult Parse(string script)
    {
        var lines = ScriptLineNormalizer.NormalizeWithSourceLines(script);
        return Parse(lines);
    }

    public ScriptParseResult Parse(IReadOnlyList<ScriptNormalizedLine> sourceLines)
    {
        var lines = sourceLines.ToArray();
        var parseResult = ParseActions(lines, 0, stopAtBlockEnd: false);
        if (!parseResult.Success)
        {
            return ScriptParseResult.Fail(parseResult.Error ?? "Could not parse script.");
        }

        return ScriptParseResult.Ok(parseResult.Actions);
    }

    private static ActionListParseResult ParseActions(ScriptNormalizedLine[] lines, int startIndex, bool stopAtBlockEnd)
    {
        var actions = new List<BrowserScriptAction>();

        for (var index = startIndex; index < lines.Length; index++)
        {
            var lineNumber = lines[index].SourceLineNumber;
            var rawLine = lines[index].Text;
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
            if (trimmed.StartsWith('}'))
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
                var equalsIndex = token.Value.IndexOf('=');
                if (!token.StartedQuoted && equalsIndex > 0 && IsOptionKey(token.Value[..equalsIndex]))
                {
                    options[token.Value[..equalsIndex]] = token.Value[(equalsIndex + 1)..];
                    continue;
                }

                positional.Add(token.Value);
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
                tokens[0].Value,
                positional,
                options,
                children));
        }

        if (stopAtBlockEnd)
        {
            var lineNumber = lines.Length is 0 ? 1 : lines[^1].SourceLineNumber;
            return ActionListParseResult.Fail($"Line {lineNumber}: missing block close '}}'.");
        }

        return ActionListParseResult.Ok(actions, lines.Length);
    }

    private static TokenizeResult Tokenize(string line, int lineNumber)
    {
        var tokens = new List<ScriptToken>();
        var current = new List<char>();
        var inQuotes = false;
        var tokenStarted = false;
        var tokenStartedQuoted = false;

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
                if (!tokenStarted)
                {
                    tokenStartedQuoted = true;
                }

                inQuotes = !inQuotes;
                tokenStarted = true;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddCurrentToken(tokens, current, ref tokenStarted, ref tokenStartedQuoted);
                continue;
            }

            tokenStarted = true;
            current.Add(character);
        }

        if (inQuotes)
        {
            return TokenizeResult.Fail($"Line {lineNumber}: unterminated quoted string.");
        }

        AddCurrentToken(tokens, current, ref tokenStarted, ref tokenStartedQuoted);

        return TokenizeResult.Ok(tokens);
    }

    private static void AddCurrentToken(List<ScriptToken> tokens, List<char> current, ref bool tokenStarted, ref bool tokenStartedQuoted)
    {
        if (!tokenStarted)
        {
            return;
        }

        tokens.Add(new ScriptToken(new string(current.ToArray()), tokenStartedQuoted));
        current.Clear();
        tokenStarted = false;
        tokenStartedQuoted = false;
    }

    private static bool IsOptionKey(string value)
    {
        if (value.Length is 0 || !char.IsLetter(value[0]))
        {
            return false;
        }

        return value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
    }
}

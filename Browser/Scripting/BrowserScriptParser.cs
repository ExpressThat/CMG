namespace CMG.Browser.Scripting;

public sealed class BrowserScriptParser
{
    public ScriptParseResult Parse(string script)
    {
        var actions = new List<BrowserScriptAction>();
        var lines = script.ReplaceLineEndings("\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var rawLine = lines[index];
            var trimmed = rawLine.Trim();

            if (trimmed.Length is 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var tokenResult = Tokenize(trimmed, lineNumber);
            if (!tokenResult.Success)
            {
                return ScriptParseResult.Fail(tokenResult.Error ?? $"Line {lineNumber}: invalid syntax.");
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

            actions.Add(new BrowserScriptAction(
                lineNumber,
                rawLine,
                tokens[0],
                positional,
                options));
        }

        return ScriptParseResult.Ok(actions);
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
    IReadOnlyDictionary<string, string> Options);

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

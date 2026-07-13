namespace CMG.Browser.Scripting;

public static class ScriptLineNormalizer
{
    public static string[] Normalize(string script)
    {
        var lines = new List<string>();
        foreach (var sourceLine in script.ReplaceLineEndings("\n").Split('\n'))
        {
            SplitLine(sourceLine, lines);
        }

        return lines.ToArray();
    }

    public static ScriptNormalizedLine[] NormalizeWithSourceLines(string script)
    {
        var normalized = new List<ScriptNormalizedLine>();
        var sourceLines = script.ReplaceLineEndings("\n").Split('\n');
        for (var index = 0; index < sourceLines.Length; index++)
        {
            var logicalLines = new List<string>();
            SplitLine(sourceLines[index], logicalLines);
            if (logicalLines.Count is 0)
            {
                normalized.Add(new ScriptNormalizedLine(string.Empty, index + 1));
                continue;
            }

            normalized.AddRange(logicalLines.Select(line => new ScriptNormalizedLine(line, index + 1)));
        }

        return normalized.ToArray();
    }

    private static void SplitLine(string line, List<string> lines)
    {
        var current = new List<char>();
        var inQuotes = false;
        var inVariable = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character is '\\' && inQuotes && index + 1 < line.Length)
            {
                current.Add(character);
                current.Add(line[++index]);
                continue;
            }

            if (character is '"')
            {
                inQuotes = !inQuotes;
                current.Add(character);
                continue;
            }

            if (!inQuotes && character is '$' && index + 1 < line.Length && line[index + 1] is '{')
            {
                current.Add(character);
                current.Add(line[++index]);
                inVariable = true;
                continue;
            }

            if (!inQuotes && !inVariable && character is '#' && IsCommentStart(line, index))
            {
                break;
            }

            if (!inQuotes && inVariable)
            {
                current.Add(character);
                if (character is '}')
                {
                    inVariable = false;
                }

                continue;
            }

            if (!inQuotes && character is '{')
            {
                current.Add(character);
                AddIfNotBlank(lines, current);
                current.Clear();
                continue;
            }

            if (!inQuotes && character is '}')
            {
                AddIfNotBlank(lines, current);
                current.Clear();
                lines.Add("}");
                continue;
            }

            if (!inQuotes && character is ';')
            {
                AddIfNotBlank(lines, current);
                current.Clear();
                continue;
            }

            current.Add(character);
        }

        AddIfNotBlank(lines, current);
    }

    private static bool IsCommentStart(string line, int index)
    {
        if (index is 0)
        {
            return true;
        }

        return char.IsWhiteSpace(line[index - 1]) &&
            (index + 1 == line.Length || char.IsWhiteSpace(line[index + 1]));
    }

    private static void AddIfNotBlank(List<string> lines, List<char> current)
    {
        var text = new string(current.ToArray());
        if (!string.IsNullOrWhiteSpace(text))
        {
            lines.Add(text);
        }
    }
}

public sealed record ScriptNormalizedLine(string Text, int SourceLineNumber);

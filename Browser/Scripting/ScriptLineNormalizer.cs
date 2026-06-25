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

    private static void SplitLine(string line, List<string> lines)
    {
        var current = new List<char>();
        var inQuotes = false;
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

            current.Add(character);
        }

        AddIfNotBlank(lines, current);
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

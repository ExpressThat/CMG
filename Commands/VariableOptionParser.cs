namespace CMG.Commands;

internal static class VariableOptionParser
{
    public static bool TryParse(
        IEnumerable<string> values,
        out IReadOnlyDictionary<string, string> variables,
        out string? error)
    {
        var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            var separator = value.IndexOf('=');
            if (separator <= 0)
            {
                variables = parsed;
                error = $"Variable '{value}' must use name=value.";
                return false;
            }

            var name = value[..separator].Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                variables = parsed;
                error = $"Variable '{value}' must include a non-empty name.";
                return false;
            }

            parsed[name] = value[(separator + 1)..];
        }

        variables = parsed;
        error = null;
        return true;
    }
}

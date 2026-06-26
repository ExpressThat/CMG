namespace CMG.Runner;

public static class CmgDeclarationAliases
{
    public static CmgDeclaration Normalize(string kind, IReadOnlyDictionary<string, string> options)
    {
        var parts = kind.Split('.', 2);
        if (parts.Length is not 2 || !IsDeclaration(parts[0]))
        {
            return new(kind, options);
        }

        var normalized = new Dictionary<string, string>(options, StringComparer.OrdinalIgnoreCase);
        switch (parts[1].ToLowerInvariant())
        {
            case "only":
                normalized["only"] = "true";
                break;
            case "skip":
                normalized["skip"] = "true";
                normalized.TryAdd("reason", $"Skipped by {kind}.");
                break;
            case "fixme":
                normalized["skip"] = "true";
                normalized.TryAdd("reason", $"Marked fixme by {kind}.");
                break;
            case "todo":
                normalized["skip"] = "true";
                normalized.TryAdd("reason", $"Marked todo by {kind}.");
                break;
            case "slow":
                normalized["slow"] = "true";
                break;
            default:
                return new(kind, options);
        }

        return new(parts[0], normalized);
    }

    private static bool IsDeclaration(string kind) =>
        kind.Equals("test", StringComparison.OrdinalIgnoreCase) ||
        kind.Equals("it", StringComparison.OrdinalIgnoreCase) ||
        kind.Equals("specify", StringComparison.OrdinalIgnoreCase) ||
        kind.Equals("suite", StringComparison.OrdinalIgnoreCase) ||
        kind.Equals("describe", StringComparison.OrdinalIgnoreCase) ||
        kind.Equals("context", StringComparison.OrdinalIgnoreCase);
}

public sealed record CmgDeclaration(string Kind, IReadOnlyDictionary<string, string> Options);

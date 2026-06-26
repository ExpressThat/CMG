namespace CMG.Runner;

internal static class CmgVariables
{
    private const string Prefix = "var.";

    public static IReadOnlyList<CmgNode> FromRunOptions(CmgRunOptions options) =>
        SetNodes(lineNumber: 0, options.Variables);

    public static IReadOnlyList<CmgNode> FromDeclarationOptions(
        int lineNumber,
        IReadOnlyDictionary<string, string> options)
    {
        var variables = options
            .Where(option => option.Key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            .Select(option => (option.Key[Prefix.Length..], option.Value));

        return SetNodes(lineNumber, variables);
    }

    private static IReadOnlyList<CmgNode> SetNodes(
        int lineNumber,
        IEnumerable<KeyValuePair<string, string>> variables) =>
        SetNodes(lineNumber, variables.Select(pair => (pair.Key, pair.Value)));

    private static IReadOnlyList<CmgNode> SetNodes(
        int lineNumber,
        IEnumerable<(string Key, string Value)> variables) =>
        variables
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
            .Select(pair => new CmgNode(
                lineNumber,
                "set",
                "set",
                [pair.Key, pair.Value],
                new Dictionary<string, string>(),
                []))
            .ToArray();
}

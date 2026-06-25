namespace CMG.Runner;

internal sealed record CmgNodeListResult(
    bool Success,
    IReadOnlyList<CmgNode> Nodes,
    int NextIndex,
    string? Error)
{
    public static CmgNodeListResult Ok(IReadOnlyList<CmgNode> nodes, int nextIndex) =>
        new(true, nodes, nextIndex, null);

    public static CmgNodeListResult Fail(string error) => new(false, [], 0, error);
}

internal sealed record CmgTokenizeResult(
    bool Success,
    IReadOnlyList<string> Tokens,
    string? Error)
{
    public static CmgTokenizeResult Ok(IReadOnlyList<string> tokens) => new(true, tokens, null);

    public static CmgTokenizeResult Fail(string error) => new(false, [], error);
}

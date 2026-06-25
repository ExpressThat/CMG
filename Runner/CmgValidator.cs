namespace CMG.Runner;

public sealed class CmgValidator
{
    public CmgValidationResult Validate(CmgTestCase test)
    {
        foreach (var action in Flatten(test.Actions))
        {
            foreach (var argument in action.Arguments)
            {
                if (!LooksLikeLocator(argument) || CmgLocator.IsSupported(argument))
                {
                    continue;
                }

                return CmgValidationResult.Fail(action.LineNumber, action.Kind, CmgLocator.UnsupportedReason(argument));
            }
        }

        return CmgValidationResult.Ok();
    }

    private static IEnumerable<CmgNode> Flatten(IEnumerable<CmgNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Flatten(node.Children))
            {
                yield return child;
            }
        }
    }

    private static bool LooksLikeLocator(string value) =>
        value.Contains('=', StringComparison.Ordinal) || value.StartsWith('#') || value.StartsWith('.');
}

public sealed record CmgValidationResult(bool Success, int LineNumber, string Action, string? Error)
{
    public static CmgValidationResult Ok() => new(true, 0, string.Empty, null);

    public static CmgValidationResult Fail(int lineNumber, string action, string error) =>
        new(false, lineNumber, action, error);
}

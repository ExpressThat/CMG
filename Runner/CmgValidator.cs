namespace CMG.Runner;

public sealed class CmgValidator
{
    public CmgValidationResult Validate(CmgTestCase test)
    {
        foreach (var action in Flatten(test.Actions))
        {
            if (!ValidatesLocator(action.Kind))
            {
                continue;
            }

            foreach (var argument in action.Arguments.Take(1))
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

    private static bool ValidatesLocator(string kind) =>
        kind.ToLowerInvariant() is
            "click" or "type" or "presssequentially" or "clear" or "hover" or "scrollintoview" or "select" or "selectoption" or
            "html" or "screenshot" or "asserttext" or "download" or "uploadfiles" or "setinputfiles" or "selectfile" or
            "fill" or "check" or "uncheck" or "focus" or "blur" or "selecttext" or
            "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "dispatchevent" or "expecttext" or "expectvalue" or
            "expectattribute" or "expectchecked" or "expectcount" or "expectscreenshot" or "tohavescreenshot" or "dragto" or "tocontaintext" or
            "evaluateonselector" or "evalonselector" or "evaluateall" or "evalall" or
            "textcontent" or "innertext" or "inputvalue" or "getattribute" or
            "waitforvisible" or "waitforhidden";
}

public sealed record CmgValidationResult(bool Success, int LineNumber, string Action, string? Error)
{
    public static CmgValidationResult Ok() => new(true, 0, string.Empty, null);

    public static CmgValidationResult Fail(int lineNumber, string action, string error) =>
        new(false, lineNumber, action, error);
}

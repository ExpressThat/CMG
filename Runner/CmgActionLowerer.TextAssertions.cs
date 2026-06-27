namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    private static IReadOnlyList<string> LowerTextAssertion(CmgNode action)
    {
        action = NormalizeLocatorOption(action);
        if (action.Arguments.Count is 1 && BodyTextAssertionNames.Contains(action.Kind.ToLowerInvariant()))
        {
            return [ToLine(action.Kind, action.Arguments, action.Options)];
        }

        if (action.Arguments.Count is 0)
        {
            return [ToLine("assertText", action.Arguments, action.Options)];
        }

        var resolved = CmgLocator.Resolve(action.Arguments[0], action.LineNumber);
        var command = IsNegativeTextAssertion(action.Kind) ? action.Kind : "assertText";
        return [
            .. resolved.PrefixLines,
            ToLine(command, [resolved.Selector, .. action.Arguments.Skip(1)], action.Options)
        ];
    }

    private static readonly HashSet<string> BodyTextAssertionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "contains",
        "tocontaintext",
        "notcontains",
        "tonotcontaintext"
    };

    private static bool IsNegativeTextAssertion(string name) =>
        name.ToLowerInvariant() is "expectnotext" or "expectnottext" or "notcontains" or
            "notcontainstext" or "tonotcontaintext" or "tohavenotext" or "tohavenottext";
}

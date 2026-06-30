namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static bool ShouldExpandBeforeDispatch(string name) =>
        !name.Equals("if", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("elseif", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("while", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("until", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("doWhile", StringComparison.OrdinalIgnoreCase) &&
        !name.Equals("doUntil", StringComparison.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string> EmptyVariables =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

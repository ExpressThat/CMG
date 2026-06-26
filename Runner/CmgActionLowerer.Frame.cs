namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    private static string LowerFrameAction(CmgNode action)
    {
        var normalized = NormalizeFrameLocatorOption(action);
        return ToLine(action.Kind, normalized.Arguments, normalized.Options);
    }

    private static CmgNode NormalizeFrameLocatorOption(CmgNode action)
    {
        var locator = action.Options.FirstOrDefault(pair => CmgLocatorKeys.IsLocatorOption(pair.Key));
        if (string.IsNullOrWhiteSpace(locator.Key) || action.Arguments.Count is 0)
        {
            return action;
        }

        var options = action.Options.Where(pair => !pair.Key.Equals(locator.Key, StringComparison.Ordinal)).ToDictionary();
        return action with
        {
            Arguments = [action.Arguments[0], CmgLocatorKeys.Format(locator.Key, locator.Value), .. action.Arguments.Skip(1)],
            Options = options
        };
    }
}

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<BrowserScriptAction> CollectBranches(IReadOnlyList<BrowserScriptAction> actions, ref int index)
    {
        var branches = new List<BrowserScriptAction> { actions[index] };
        while (index + 1 < actions.Count && IsConditionalBranch(actions[index + 1].Name))
        {
            branches.Add(actions[++index]);
        }

        return branches;
    }

    private static bool IsConditionalBranch(string name) =>
        name.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("else", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<BrowserScriptAction> CollectTryBranches(IReadOnlyList<BrowserScriptAction> actions, ref int index)
    {
        var branches = new List<BrowserScriptAction> { actions[index] };
        while (index + 1 < actions.Count && IsTryBranch(actions[index + 1].Name))
        {
            branches.Add(actions[++index]);
        }

        return branches;
    }

    private static bool IsTryBranch(string name) =>
        name.Equals("catch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("finally", StringComparison.OrdinalIgnoreCase);

    private static bool IsSwitchBranch(string name) =>
        name.Equals("case", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("default", StringComparison.OrdinalIgnoreCase);

    private static bool ShouldCaptureAfterAction(BrowserScriptAction action) =>
        !action.Name.Equals("pauseGif", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("recordCheckpoint", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("intro", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("outro", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("showPointer", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("hidePointer", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("hideFromGif", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("cutGif", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("speedUpGif", StringComparison.OrdinalIgnoreCase) &&
        !action.Name.Equals("slowDownGif", StringComparison.OrdinalIgnoreCase);
}

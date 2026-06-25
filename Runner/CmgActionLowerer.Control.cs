namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    private IReadOnlyList<string> LowerControlBlock(CmgNode action)
    {
        if (action.Children.Count is 0)
        {
            return [ToLine(action.Kind, action.Arguments, action.Options)];
        }

        return [
            ToLine(action.Kind, action.Arguments, action.Options) + " {",
            .. action.Children.SelectMany(Lower),
            "}"
        ];
    }
}

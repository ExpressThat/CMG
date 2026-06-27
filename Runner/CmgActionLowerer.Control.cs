namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    public IReadOnlyList<string> LowerRecordingBlock(CmgNode action)
    {
        return [
            ToLine(action.Kind, action.Arguments, action.Options) + " {",
            .. action.Children.SelectMany(Lower),
            "}"
        ];
    }

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

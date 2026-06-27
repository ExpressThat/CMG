namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    private static string LowerViewportAlias(CmgNode action)
    {
        if (action.Arguments.Count is 0)
        {
            return ToLine("setViewport", [], action.Options);
        }

        if (action.Arguments.Count is 2 &&
            !action.Options.ContainsKey("width") &&
            !action.Options.ContainsKey("height"))
        {
            var options = new Dictionary<string, string>(action.Options)
            {
                ["width"] = action.Arguments[0],
                ["height"] = action.Arguments[1]
            };
            return ToLine("setViewport", [], options);
        }

        return ToLine("setViewport", action.Arguments, action.Options);
    }
}

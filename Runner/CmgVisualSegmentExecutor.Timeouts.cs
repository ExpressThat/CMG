using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static ScriptTimeoutOptions? BuildTimeoutOptions(CmgRunOptions options)
    {
        return options.DefaultTimeout is null && options.NavigationTimeout is null && options.AssertionTimeout is null
            ? null
            : new ScriptTimeoutOptions(options.DefaultTimeout, options.NavigationTimeout, options.AssertionTimeout);
    }

    private static CmgNode ApplyRunTimeoutDefault(CmgNode action, CmgRunOptions options) =>
        ApplyRunTimeoutDefault(action, BuildTimeoutOptions(options));

    private static CmgNode ApplyRunTimeoutDefault(CmgNode action, ScriptTimeoutOptions? timeouts)
    {
        if (timeouts?.DefaultTimeout is null || action.Options.ContainsKey("timeout"))
        {
            return action;
        }

        var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase)
        {
            ["timeout"] = timeouts.DefaultTimeout.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        return action with { Options = options };
    }
}

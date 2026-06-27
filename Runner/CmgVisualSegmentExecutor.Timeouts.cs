using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private const int BuiltInTimeout = 5_000;

    private static ScriptTimeoutOptions? BuildTimeoutOptions(CmgTestCase test, CmgRunOptions options)
    {
        var multiplier = SlowMultiplier(test);
        if (multiplier is null)
        {
            return BuildTimeoutOptions(options);
        }

        var value = multiplier.Value;
        return new ScriptTimeoutOptions(
            Scale(options.DefaultTimeout ?? BuiltInTimeout, value),
            Scale(options.NavigationTimeout ?? options.DefaultTimeout ?? BuiltInTimeout, value),
            Scale(options.AssertionTimeout ?? options.DefaultTimeout ?? BuiltInTimeout, value));
    }

    private static ScriptTimeoutOptions? BuildTimeoutOptions(CmgRunOptions options) =>
        options.DefaultTimeout is null && options.NavigationTimeout is null && options.AssertionTimeout is null
            ? null
            : new ScriptTimeoutOptions(options.DefaultTimeout, options.NavigationTimeout, options.AssertionTimeout);

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

    private static double? SlowMultiplier(CmgTestCase test)
    {
        if (!test.Options.TryGetValue("slow", out var value) || IsFalse(value))
        {
            return null;
        }

        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var multiplier) && multiplier > 0
            ? multiplier
            : 3;
    }

    private static int Scale(int timeout, double multiplier) =>
        (int)Math.Min(int.MaxValue, Math.Round(timeout * multiplier));

    private static bool IsFalse(string value) =>
        value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("0", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("no", StringComparison.OrdinalIgnoreCase);
}

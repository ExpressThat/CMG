using CMG.Browser;

namespace CMG.Runner;

public sealed class CmgVisualAssertionRunner
{
    public CmgStepResult Run(CmgNode action, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        var baseline = action.Options.TryGetValue("baseline", out var baselinePath)
            ? baselinePath
            : action.Arguments.ElementAtOrDefault(1) ?? "baseline.png";
        var output = action.Options.TryGetValue("output", out var outputPath) ? outputPath : "actual.png";
        var tolerance = action.Options.TryGetValue("tolerance", out var toleranceValue) && double.TryParse(toleranceValue, out var parsed)
            ? parsed
            : 0;
        if (!TryBoolOption(action, "fullPage", out var fullPage))
        {
            return Fail(action, $"{action.Kind} option fullPage= must be true or false.", output);
        }

        try
        {
            var targetSelector = action.Arguments.Count > 0 ? ResolveVisualSelector(action.Arguments[0], action.LineNumber, remoteDebuggingUrl, automationClient) : null;
            var targetBox = targetSelector is not null ? automationClient.GetElementBox(remoteDebuggingUrl, targetSelector) : null;
            var actual = action.Arguments.Count > 0
                ? automationClient.GetElementScreenshot(remoteDebuggingUrl, targetSelector!)
                : automationClient.GetPageScreenshot(remoteDebuggingUrl, fullPage: fullPage);
            actual = ApplyMasks(action, remoteDebuggingUrl, automationClient, actual, targetBox);
            WriteBytes(output, actual);
            if (!File.Exists(baseline))
            {
                WriteBytes(baseline, actual);
                return Fail(action, $"Baseline '{baseline}' did not exist. Created it from the actual screenshot.", output);
            }

            var difference = CmgPngComparer.Compare(File.ReadAllBytes(baseline), actual);
            return difference <= tolerance
                ? Pass(action, $"VISUAL {action.LineNumber:000} diff={difference:0.####}")
                : Fail(action, $"Screenshot diff {difference:0.####} exceeded tolerance {tolerance:0.####}.", output);
        }
        catch (Exception exception) when (exception is IOException or ChromeDevToolsException or ElementNotFoundException or ArgumentException)
        {
            return Fail(action, exception.Message, output);
        }
    }

    private static byte[] ApplyMasks(CmgNode action, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, byte[] actual, ElementBox? origin)
    {
        if (!action.Options.TryGetValue("mask", out var masks) || string.IsNullOrWhiteSpace(masks))
        {
            return actual;
        }

        var boxes = masks.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(selector => automationClient.GetElementBox(remoteDebuggingUrl, ResolveVisualSelector(selector, action.LineNumber, remoteDebuggingUrl, automationClient)))
            .ToArray();
        var color = action.Options.TryGetValue("maskColor", out var maskColor) ? maskColor : "#ff00ff";
        return CmgScreenshotMasker.Apply(actual, boxes, color, origin);
    }

    private static string ResolveVisualSelector(string selector, int lineNumber, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        foreach (var expression in CmgLocator.PrefixExpressions(selector, lineNumber))
        {
            automationClient.Evaluate(remoteDebuggingUrl, expression);
        }

        return CmgLocator.Resolve(selector, lineNumber).Selector;
    }

    private static bool TryBoolOption(CmgNode action, string name, out bool value)
    {
        if (!action.Options.TryGetValue(name, out var raw))
        {
            value = false;
            return true;
        }

        return bool.TryParse(raw, out value);
    }

    private static void WriteBytes(string path, byte[] bytes)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
        File.WriteAllBytes(fullPath, bytes);
    }

    private static CmgStepResult Pass(CmgNode action, string output) => new(action.LineNumber, action.Kind, true, [output], null, null);

    private static CmgStepResult Fail(CmgNode action, string error, string? output) =>
        new(action.LineNumber, action.Kind, false, output is null ? [] : [$"VISUAL_ACTUAL {action.LineNumber:000} {Path.GetFullPath(output)}"], error, null);
}

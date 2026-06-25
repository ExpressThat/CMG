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

        try
        {
            var actual = action.Arguments.Count > 0
                ? automationClient.GetElementScreenshot(remoteDebuggingUrl, CmgLocator.ToCssSelector(action.Arguments[0]))
                : automationClient.GetPageScreenshot(remoteDebuggingUrl);
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
        catch (Exception exception) when (exception is IOException or ChromeDevToolsException or ElementNotFoundException)
        {
            return Fail(action, exception.Message, output);
        }
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

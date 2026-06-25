namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecutePrintPdf(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var path = RequiredPdfPath(action);
        var options = new PdfPrintOptions(
            GetBoolOption(action, "landscape"),
            GetBoolOption(action, "printBackground", defaultValue: true),
            GetDoubleOption(action, "scale", 1));
        var bytes = automationClient.PrintPdf(remoteDebuggingUrl, options);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
        File.WriteAllBytes(path, bytes);
        return [$"PDF {action.LineNumber:000} {path}"];
    }

    private static string RequiredPdfPath(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("path", out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ScriptExecutionException($"{action.Name} requires path=<file>.");
        }

        return Path.GetFullPath(value);
    }

    private static bool GetBoolOption(BrowserScriptAction action, string name, bool defaultValue = false)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        throw new ScriptExecutionException($"{action.Name} option {name}= must be true or false.");
    }

    private static double GetDoubleOption(BrowserScriptAction action, string name, double defaultValue)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            return defaultValue;
        }

        return double.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : throw new ScriptExecutionException($"{action.Name} option {name}= must be a positive number.");
    }
}

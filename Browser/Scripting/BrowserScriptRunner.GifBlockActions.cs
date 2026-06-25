using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteGifBlock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? commandRecorder)
    {
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException("gif requires a block body.");
        }

        var recorder = commandRecorder ?? new ScriptGifRecorder(automationClient, new ScriptRecordingOptions(GifBlockPath(action)));
        var output = new List<string>();
        if (commandRecorder is null)
        {
            recorder.Start(remoteDebuggingUrl);
        }
        else
        {
            output.Add($"GIF_BLOCK_SUPPRESSED {action.LineNumber:000}");
        }

        try
        {
            foreach (var child in action.Children)
            {
                recorder.BeforeAction(child);
                var lines = ExecuteAction(remoteDebuggingUrl, automationClient, child, context, recorder);
                recorder.AfterAction(child);
                output.AddRange(lines);
            }
        }
        finally
        {
            if (commandRecorder is null)
            {
                FinishRecording(recorder, output);
                recorder.Dispose();
            }
        }

        return output;
    }

    private static string GifBlockPath(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            return output;
        }

        var name = action.Arguments.Count > 0 ? action.Arguments[0] : $"gif-{action.LineNumber:000}";
        return Path.Combine(Directory.GetCurrentDirectory(), $"{SafeFileName(name)}.gif");
    }

    private static string SafeFileName(string value)
    {
        var safe = string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? character : '-')).Trim('-');
        return string.IsNullOrWhiteSpace(safe) ? "gif-block" : safe;
    }

    private static bool IsRecordingBlock(string name) =>
        name.Equals("gif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("recordVideo", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("screencast", StringComparison.OrdinalIgnoreCase);
}

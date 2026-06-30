using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public interface IBrowserControlService
{
    ElementResult GetElement(BrowserKind browserKind, string selector, ElementOutputMode outputMode);

    ElementResult GetElement(BrowserKind browserKind, int? port, string selector, ElementOutputMode outputMode) =>
        GetElement(browserKind, selector, outputMode);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace) =>
        RunScript(browserKind, file, gif);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts) =>
        RunScript(browserKind, file, gif, trace);

    ScriptRunResult RunScript(
        BrowserKind browserKind,
        int? port,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null) =>
        RunScript(browserKind, file, gif, trace, timeouts, baseUrl, variables);

    ScriptRunResult RunScript(
        BrowserKind browserKind,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables) =>
        RunScript(browserKind, file, gif, trace, timeouts);

    ScriptRunResult RunScriptAction(BrowserKind browserKind, string scriptLine);

    ScriptRunResult RunScriptAction(BrowserKind browserKind, int? port, string scriptLine) =>
        RunScriptAction(browserKind, scriptLine);

    ScriptRunResult RunScriptText(
        BrowserKind browserKind,
        int? port,
        string script,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null);
}

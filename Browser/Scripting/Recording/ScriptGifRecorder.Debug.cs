namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private GifDebugOptions CurrentDebug(BrowserScriptAction? action = null) =>
        GifDebugOptions.FromOptions(
            action?.Options ?? activeAction?.Options ?? EmptyDebugOptions,
            $"{action?.Name ?? activeAction?.Name ?? "gif"} option",
            options.EffectiveEncoding.Diagnostics);

    private void RecordDebugFrame(int delayMilliseconds, string kind, BrowserScriptAction? action = null)
    {
        var debug = CurrentDebug(action);
        if (!debug.Action && !debug.Context && !debug.Target && !debug.Coordinates && !debug.Scroll) return;
        var source = action ?? activeAction;
        debugFrames.Add(new GifDebugFrame(
            frameSink.FrameCount,
            frameSink.DurationMilliseconds,
            delayMilliseconds,
            kind,
            source?.Name,
            source?.LineNumber,
            activeExecutionContext,
            source is null ? null : DebugTarget(source),
            pointer.Position));
    }

    private void PrepareDebugEvidence()
    {
        if (remoteDebuggingUrl is null || activeAction is null) return;
        var debug = GifDebugOptions.FromOptions(
            activeAction.Options,
            $"{activeAction.Name} option",
            options.EffectiveEncoding.Diagnostics);
        if (!debug.Action && !debug.Context && !debug.Target && !debug.Coordinates && !debug.Scroll) return;

        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowGifDebugEvidence(
            debug.Action ? activeAction.Name : null,
            activeAction.LineNumber,
            debug.Context ? activeExecutionContext : null,
            debug.Target ? DebugTarget(activeAction) : null,
            debug.Coordinates ? pointer.Position : null,
            debug.Scroll));
    }

    private string? DebugTarget(BrowserScriptAction action)
    {
        if (action.Arguments.Count is 0 || !HasElementTarget(action.Name)) return null;
        try { return ResolveLocator(action.Arguments[0], action.LineNumber); }
        catch (ScriptExecutionException) { return null; }
    }

    private void RemoveDebugEvidence()
    {
        if (remoteDebuggingUrl is null) return;
        try { devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveGifDebugEvidence()); }
        catch (ChromeDevToolsException) { }
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyDebugOptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private bool captureAfterAction;
    private ElementPoint? teleportOrigin;
    private int pointerIdlePhase;
    private BrowserScriptAction? frameAction;

    private GifPointerEvidenceOptions PointerEvidenceFor(BrowserScriptAction? action) =>
        action is null
            ? options.EffectivePointerEvidence
            : options.EffectivePointerEvidence.WithOptions(action.Options, $"{action.Name} option");

    private void PreparePointerEvidence()
    {
        if (remoteDebuggingUrl is null) return;
        var action = frameAction ?? activeAction;
        var evidence = PointerEvidenceFor(action);
        var focusPulse = captureAfterAction && evidence.FocusPulse && action is not null && IsFocusProducingAction(action.Name);
        var selector = action is null ? null : AccessibilityTarget(action);
        var tabs = TabContextLabel(evidence);
        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowGifPointerEvidence(
            pointer.Position,
            selector,
            evidence,
            focusPulse,
            evidence.TeleportMarker ? teleportOrigin : null,
            evidence.Idle is PointerIdleMode.Pulse ? pointerIdlePhase : 0,
            evidence.Contrast is PointerContrastMode.Auto && cursorVisual.Color is null,
            tabs));
    }

    private string? TabContextLabel(GifPointerEvidenceOptions evidence)
    {
        if (evidence.TabContext is PointerTargetCalloutMode.None) return null;
        var count = devToolsClient.ListTabs(remoteDebuggingUrl!).Count;
        return evidence.TabContext is PointerTargetCalloutMode.Always || count > 1
            ? count <= 1 ? "Current tab" : $"{count} tabs"
            : null;
    }

    private static bool IsFocusProducingAction(string name) => name.ToLowerInvariant() is
        "focus" or "click" or "tap" or "touchtap" or "type" or "presssequentially" or "fill" or "clear" or
        "check" or "uncheck" or "select" or "selectoption" or "press" or "keydown" or "keyup" or
        "keyboardshortcut" or "shortcut" or "hotkey" or "inserttext";

    private void RemovePointerEvidence()
    {
        if (remoteDebuggingUrl is null) return;
        try { devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveGifPointerEvidence()); }
        catch (ChromeDevToolsException) { }
    }

    public void SetMouseButtonState(BrowserScriptAction action, bool pressed)
    {
        cursorPressed = pressed;
        cursorTrail = false;
        cursorBreadcrumb = false;
        cursorVisual = options.EffectivePointerVisual.WithAction(action, touch: false);
        if (remoteDebuggingUrl is not null && ShouldShowPointer(action))
        {
            devToolsClient.MoveDomCursor(remoteDebuggingUrl, pointer.Position, pressed: pressed, visual: cursorVisual);
        }
    }
}

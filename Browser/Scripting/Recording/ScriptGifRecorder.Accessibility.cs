namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void PrepareAccessibilityEvidence()
    {
        if (remoteDebuggingUrl is null || activeAction is null) return;
        var accessibility = EffectiveAccessibility(activeAction);
        if (!accessibility.ShowKeystrokes && !accessibility.ShowMouseButtons && !accessibility.FocusEvidence &&
            !accessibility.AccessibleNames && !accessibility.ContrastWarnings) return;

        var inputLabel = accessibility.ShowKeystrokes ? KeystrokeLabel(activeAction) : null;
        if (accessibility.ShowMouseButtons) inputLabel ??= MouseButtonLabel(activeAction);

        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowGifAccessibilityEvidence(
            AccessibilityTarget(activeAction),
            inputLabel,
            accessibility.FocusEvidence,
            accessibility.AccessibleNames,
            accessibility.HighContrast,
            accessibility.ContrastWarnings));
    }

    private GifAccessibilityOptions EffectiveAccessibility(BrowserScriptAction action)
    {
        var defaults = options.EffectiveAccessibility;
        var scoped = GifAccessibilityOptions.FromOptions(action.Options, $"{action.Name} option");
        if (action.Options.ContainsKey("accessibilityEvidence")) return scoped;
        return scoped with
        {
            ShowKeystrokes = action.Options.ContainsKey("showKeystrokes") ? scoped.ShowKeystrokes : defaults.ShowKeystrokes,
            FocusEvidence = action.Options.ContainsKey("focusEvidence") ? scoped.FocusEvidence : defaults.FocusEvidence,
            AccessibleNames = action.Options.ContainsKey("accessibleNames") ? scoped.AccessibleNames : defaults.AccessibleNames,
            HighContrast = action.Options.ContainsKey("highContrast") ? scoped.HighContrast : defaults.HighContrast,
            ContrastWarnings = action.Options.ContainsKey("contrastWarnings") ? scoped.ContrastWarnings : defaults.ContrastWarnings,
            ShowMouseButtons = action.Options.ContainsKey("showMouseButtons") ? scoped.ShowMouseButtons : defaults.ShowMouseButtons
        };
    }

    private string? AccessibilityTarget(BrowserScriptAction action)
    {
        if (action.Arguments.Count is 0 || !HasElementTarget(action.Name)) return null;
        try { return ResolveLocator(action.Arguments[0], action.LineNumber); }
        catch (ScriptExecutionException) { return null; }
    }

    private static string? KeystrokeLabel(BrowserScriptAction action) =>
        action.Name.ToLowerInvariant() switch
        {
            "press" or "keyboardshortcut" or "shortcut" or "hotkey" or "keydown" or "keyup" =>
                action.Arguments.Count > 0 ? action.Arguments[0] : null,
            "type" or "presssequentially" or "fill" or "inserttext" or "frametype" or "framefill" => "Text input",
            "clear" => "Clear input",
            _ => null
        };

    private static string? MouseButtonLabel(BrowserScriptAction action) =>
        action.Name.ToLowerInvariant() switch
        {
            "mousedown" => "Mouse: left down",
            "mouseup" => "Mouse: left up",
            _ => null
        };

    private static bool HasElementTarget(string name) =>
        name.ToLowerInvariant() is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or
            "tap" or "touchtap" or "type" or "presssequentially" or "fill" or "clear" or "hover" or "focus" or
            "blur" or "check" or "uncheck" or "select" or "selectoption";

    private void RemoveAccessibilityEvidence()
    {
        if (remoteDebuggingUrl is null) return;
        try { devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveGifAccessibilityEvidence()); }
        catch (ChromeDevToolsException) { }
    }
}

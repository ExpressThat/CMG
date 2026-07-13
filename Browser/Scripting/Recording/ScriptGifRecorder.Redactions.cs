namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private int nextRedactionId;

    public void AddRedaction(BrowserScriptAction action)
    {
        if (action.Arguments.Count is not 1)
        {
            throw new ScriptExecutionException($"{action.Name} requires one selector or rich locator.");
        }

        var locator = action.Arguments[0];
        var style = GifRedactionOptions.ParseStyle(
            action.Options.GetValueOrDefault("style") ?? action.Options.GetValueOrDefault("redactStyle"),
            $"{action.Name} option");
        var color = action.Options.GetValueOrDefault("color") ?? action.Options.GetValueOrDefault("redactColor") ?? "#111827";
        var replacement = action.Options.GetValueOrDefault("replacement") ?? action.Options.GetValueOrDefault("redactReplacement") ?? "[redacted]";
        var padding = ParseRedactionPadding(action);
        if (remoteDebuggingUrl is not null)
        {
            var selector = ResolveLocator(locator, action.LineNumber);
            devToolsClient.GetElementBox(remoteDebuggingUrl, selector);
        }

        var rule = new GifRedactionRule($"action-{++nextRedactionId}", locator, style, color, replacement, padding);
        redactions.RemoveAll(existing => existing.Locator.Equals(locator, StringComparison.Ordinal));
        redactions.Add(rule);
        AuditRedaction("add", rule);
    }

    public void RemoveRedaction(BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            throw new ScriptExecutionException($"{action.Name} accepts zero or one selector.");
        }

        if (action.Arguments.Count is 0)
        {
            foreach (var rule in redactions.ToArray()) AuditRedaction("clear", rule);
            redactions.Clear();
            return;
        }

        var locator = action.Arguments[0];
        foreach (var rule in redactions.Where(rule => rule.Locator.Equals(locator, StringComparison.Ordinal)).ToArray())
        {
            redactions.Remove(rule);
            AuditRedaction("remove", rule);
        }
    }

    private void PrepareRedactions()
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.PrepareGifRedactions());
        foreach (var rule in redactions.Concat(actionRedactions))
        {
            var selector = ResolveLocator(rule.Locator, 0);
            devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.AddGifRedaction(
                selector,
                rule.Id,
                rule.Style.ToString().ToLowerInvariant(),
                rule.Color,
                rule.Replacement,
                rule.Padding));
        }

        if (activeAutoRedaction is not GifAutoRedactionMode.None)
        {
            devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.AddAutomaticGifRedactions(
                activeAutoRedaction is GifAutoRedactionMode.Sensitive));
        }

        if (activeStrictRedaction)
        {
            try
            {
                devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.EnforceGifRedactionSafety());
            }
            catch (ChromeDevToolsException)
            {
                redactionCaptureBlocked = true;
                throw;
            }
        }

        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.PromoteGifEvidence());
    }

    private void RemoveRedactionOverlays()
    {
        if (remoteDebuggingUrl is null) return;
        try
        {
            devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveGifRedactions());
        }
        catch (ChromeDevToolsException)
        {
        }
    }

    private void AuditRedaction(string operation, GifRedactionRule rule) =>
        redactionAudit.Add(new GifRedactionAuditEntry(
            operation,
            rule.Locator,
            rule.Style.ToString().ToLowerInvariant(),
            frameSink.FrameCount,
            frameSink.DurationMilliseconds));

    private void ConfigureActionRedactions(BrowserScriptAction action)
    {
        actionRedactions.Clear();
        var scoped = GifRedactionOptions.FromOptions(action.Options, $"{action.Name} option");
        activeAutoRedaction = action.Options.ContainsKey("autoRedact") ? scoped.Auto : options.EffectiveRedaction.Auto;
        activeStrictRedaction = action.Options.ContainsKey("redactionSafety") ? scoped.Strict : options.EffectiveRedaction.Strict;
        if (!action.Options.TryGetValue("redact", out var value) || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var style = GifRedactionOptions.ParseStyle(action.Options.GetValueOrDefault("redactStyle"), $"{action.Name} option");
        var color = action.Options.GetValueOrDefault("redactColor") ?? "#111827";
        var replacement = action.Options.GetValueOrDefault("redactReplacement") ?? "[redacted]";
        var padding = ParseRedactionPadding(action);
        actionRedactions.AddRange(value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((locator, index) => new GifRedactionRule($"scope-{index + 1}", locator, style, color, replacement, padding)));
    }

    private static int ParseRedactionPadding(BrowserScriptAction action)
    {
        var value = action.Options.GetValueOrDefault("padding") ?? action.Options.GetValueOrDefault("redactPadding");
        if (value is null) return 0;
        return int.TryParse(value, out var padding) && padding is >= 0 and <= 100
            ? padding
            : throw new ScriptExecutionException($"{action.Name} option padding= must be between 0 and 100.");
    }
}

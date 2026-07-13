namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private bool ShowEventCaption(BrowserScriptAction action, IReadOnlyList<string> output)
    {
        if (remoteDebuggingUrl is null || !TryEventCaption(action, output, out var message, out var severity)) return false;
        var parsed = BrowserCaptionOptions.FromOptions(action.Options, action.Name);
        var caption = parsed with
        {
            Style = HasCaptionOption(action, "captionStyle", "style") ? parsed.Style : CaptionStyle.Qa,
            Position = HasCaptionOption(action, "captionPosition", "position") ? parsed.Position : CaptionPosition.Bottom,
            Severity = HasCaptionOption(action, "captionSeverity", "severity") ? parsed.Severity : severity
        };
        devToolsClient.ShowMessageBar(remoteDebuggingUrl, message, caption);
        return true;
    }

    private bool TryEventCaption(
        BrowserScriptAction action,
        IReadOnlyList<string> output,
        out string message,
        out CaptionSeverity severity)
    {
        var enabled = options.EffectiveEventCaptions.WithOptions(action.Options, $"{action.Name} option");
        var name = EventName(action);
        severity = CaptionSeverity.Info;
        message = name switch
        {
            "dialog" when enabled.Dialogs => DialogMessage(action, output),
            "console" when enabled.Console => ConsoleMessage(action, output, out severity),
            "pageerror" when enabled.Console => PageErrorMessage(action, output, out severity),
            "network" when enabled.Network => NetworkMessage(action),
            "download" when enabled.Downloads => "Download completed",
            "upload" when enabled.Uploads => UploadMessage(output),
            _ => string.Empty
        };
        return message.Length > 0;
    }

    private static string EventName(BrowserScriptAction action)
    {
        var name = action.Name.ToLowerInvariant();
        if (name == "waitforevent" && action.Arguments.Count > 0) name = action.Arguments[0].ToLowerInvariant();
        if (name.Contains("dialog")) return "dialog";
        if (name.Contains("pageerror") || name == "page-error") return "pageerror";
        if (name.Contains("console")) return "console";
        if (name.Contains("download")) return "download";
        if (name is "uploadfiles" or "setinputfiles" or "selectfile") return "upload";
        return name.Contains("request") || name.Contains("response") || name.Contains("network") ? "network" : string.Empty;
    }

    private static string DialogMessage(BrowserScriptAction action, IReadOnlyList<string> output)
    {
        var name = action.Name.ToLowerInvariant();
        if (name == "capturedialogs") return "Dialog capture enabled";
        if (name is "setdialogbehavior" or "ondialog" or "handledialog" or "dialogbehavior")
            return output.Any(line => line.Contains("dismiss", StringComparison.OrdinalIgnoreCase))
                ? "Dialogs will be dismissed" : "Dialogs will be accepted";
        var result = string.Join(' ', output);
        if (result.Contains("\"accepted\":false", StringComparison.OrdinalIgnoreCase)) return "Dialog dismissed";
        if (result.Contains("\"type\":\"prompt\"", StringComparison.OrdinalIgnoreCase)) return "Dialog prompt submitted";
        return result.Contains("\"accepted\":true", StringComparison.OrdinalIgnoreCase)
            ? "Dialog accepted" : "Dialog observed";
    }

    private static string ConsoleMessage(BrowserScriptAction action, IReadOnlyList<string> output, out CaptionSeverity severity)
    {
        var absent = action.Name.Contains("NoConsole", StringComparison.OrdinalIgnoreCase);
        severity = absent ? CaptionSeverity.Success : CaptionSeverity.Warning;
        if (action.Name.Equals("captureConsole", StringComparison.OrdinalIgnoreCase)) return "Console capture enabled";
        if (absent) return "No matching console events";
        var count = ReadCount(output, "CONSOLE_LIST");
        return count is null ? "Console event observed" : $"Console entries: {count}";
    }

    private static string PageErrorMessage(BrowserScriptAction action, IReadOnlyList<string> output, out CaptionSeverity severity)
    {
        var absent = action.Name.Contains("NoPageError", StringComparison.OrdinalIgnoreCase);
        severity = absent ? CaptionSeverity.Success : CaptionSeverity.Error;
        if (action.Name.Equals("capturePageErrors", StringComparison.OrdinalIgnoreCase)) return "Page-error capture enabled";
        if (absent) return "No matching page errors";
        var count = ReadCount(output, "PAGE_ERROR_LIST");
        return count is null ? "Page error observed" : $"Page errors: {count}";
    }

    private static string NetworkMessage(BrowserScriptAction action)
    {
        var name = action.Name.ToLowerInvariant();
        if (name == "waitforevent" && action.Arguments.Count > 0) name = action.Arguments[0].ToLowerInvariant();
        if (name.Contains("response")) return "Network response matched";
        if (name.Contains("failed")) return "Network request failure observed";
        if (name.Contains("finished")) return "Network request completed";
        if (name.Contains("idle")) return "Network is idle";
        return "Network request matched";
    }

    private static string UploadMessage(IReadOnlyList<string> output)
    {
        var count = output.Select(line => line.Split(' ').LastOrDefault()).FirstOrDefault(value => int.TryParse(value, out _));
        return count is null ? "Files selected" : $"Selected {count} file{(count == "1" ? string.Empty : "s")}";
    }

    private static string? ReadCount(IReadOnlyList<string> output, string prefix) =>
        output.FirstOrDefault(line => line.StartsWith(prefix, StringComparison.Ordinal))?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(part => part.StartsWith("count=", StringComparison.Ordinal))?[6..];

    private static bool HasCaptionOption(BrowserScriptAction action, string primary, string alias) =>
        action.Options.ContainsKey(primary) || action.Options.ContainsKey(alias);

    private void RemoveEventCaption()
    {
        if (remoteDebuggingUrl is not null) devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveMessageBar());
    }
}

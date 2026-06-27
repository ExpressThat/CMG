using System.Text.Json;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteCaptureConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.Install());
        return [$"CONSOLE_CAPTURE {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var level = action.Options.TryGetValue("level", out var value) ? value : string.Empty;
        ValidateConsoleLevel(action, level);
        ValidateTextMatchOptions(action, action.Arguments[0]);
        var result = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.WaitFor(action.Arguments[0], level, timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"CONSOLE {action.LineNumber:000} {result}"];
    }

    private static IReadOnlyList<string> ExecuteListConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var level = action.Options.TryGetValue("level", out var value) ? value : string.Empty;
        ValidateConsoleLevel(action, level);
        var text = action.Arguments.FirstOrDefault() ?? string.Empty;
        ValidateTextMatchOptions(action, text);
        var json = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.List(text, level, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        var entries = ParseConsoleEntries(json);
        var output = new List<string> { $"CONSOLE_LIST {action.LineNumber:000} count={entries.Count}" };
        output.AddRange(entries.Select(entry => $"CONSOLE_ENTRY {action.LineNumber:000} index={entry.Index} level={entry.Level} text={OneLine(entry.Text)}"));
        return output;
    }

    private static IReadOnlyList<string> ExecuteExpectNoConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var timeout = GetIntOption(action, "timeout", 0);
        var level = action.Options.TryGetValue("level", out var levelValue) ? levelValue : "error";
        ValidateConsoleLevel(action, level);
        var text = action.Arguments.FirstOrDefault() ?? string.Empty;
        ValidateTextMatchOptions(action, text);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.ExpectNone(text, level, timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"CONSOLE_OK {action.LineNumber:000} level={level}"];
    }

    private static void ValidateConsoleLevel(BrowserScriptAction action, string level)
    {
        if (string.IsNullOrWhiteSpace(level) || level is "log" or "info" or "warn" or "error")
        {
            return;
        }

        throw new ScriptExecutionException($"{action.Name} option level= must be log, info, warn, or error.");
    }

    private static IReadOnlyList<string> ExecuteCapturePageErrors(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.InstallPageErrors());
        return [$"PAGE_ERROR_CAPTURE {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForPageError(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        ValidateTextMatchOptions(action, action.Arguments[0]);
        var result = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.WaitForPageError(action.Arguments[0], timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"PAGE_ERROR {action.LineNumber:000} {result}"];
    }

    private static IReadOnlyList<string> ExecuteListPageErrors(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var text = action.Arguments.FirstOrDefault() ?? string.Empty;
        ValidateTextMatchOptions(action, text);
        var json = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.ListPageErrors(text, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        var entries = ParsePageErrorEntries(json);
        var output = new List<string> { $"PAGE_ERROR_LIST {action.LineNumber:000} count={entries.Count}" };
        output.AddRange(entries.Select(entry => $"PAGE_ERROR_ENTRY {action.LineNumber:000} index={entry.Index} type={entry.Type} source={OneLine(entry.Source)} line={entry.Line} column={entry.Column} text={OneLine(entry.Text)}"));
        return output;
    }

    private static IReadOnlyList<string> ExecuteExpectNoPageError(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var timeout = GetIntOption(action, "timeout", 0);
        var text = action.Arguments.FirstOrDefault() ?? string.Empty;
        ValidateTextMatchOptions(action, text);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.ExpectNoPageError(text, timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"PAGE_ERROR_OK {action.LineNumber:000}"];
    }

    private static IReadOnlyList<ConsoleEntry> ParseConsoleEntries(string json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json);
        return document.RootElement.EnumerateArray()
            .Select(entry => new ConsoleEntry(
                ReadInt(entry, "index"),
                ReadString(entry, "level"),
                ReadString(entry, "text")))
            .ToArray();
    }

    private static IReadOnlyList<PageErrorEntry> ParsePageErrorEntries(string json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json);
        return document.RootElement.EnumerateArray()
            .Select(entry => new PageErrorEntry(
                ReadInt(entry, "index"),
                ReadString(entry, "type"),
                ReadString(entry, "source"),
                ReadInt(entry, "line"),
                ReadInt(entry, "column"),
                ReadString(entry, "text")))
            .ToArray();
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int ReadInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var number) ? number : 0;

    private static string OneLine(string value) =>
        value.Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);

    private sealed record ConsoleEntry(int Index, string Level, string Text);

    private sealed record PageErrorEntry(int Index, string Type, string Source, int Line, int Column, string Text);
}

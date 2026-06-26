namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecutePress(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        if (IsKeyboardChord(action.Arguments[0]))
        {
            PressShortcut(remoteDebuggingUrl, automationClient, ParseKeyboardChord(action, action.Arguments[0]), PressDelay(action));
            return [$"KEYBOARD_SHORTCUT {action.LineNumber:000} {action.Arguments[0]}"];
        }

        if (action.Options.ContainsKey("delay"))
        {
            PressWithDelay(remoteDebuggingUrl, automationClient, action.Arguments[0], PressDelay(action) ?? 0);
            return [];
        }

        automationClient.Press(remoteDebuggingUrl, action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteKeyboardAction(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        switch (action.Name.ToLowerInvariant())
        {
            case "keydown":
                automationClient.KeyDown(remoteDebuggingUrl, action.Arguments[0]);
                return [$"KEY_DOWN {action.LineNumber:000} {action.Arguments[0]}"];
            case "keyup":
                automationClient.KeyUp(remoteDebuggingUrl, action.Arguments[0]);
                return [$"KEY_UP {action.LineNumber:000} {action.Arguments[0]}"];
            case "inserttext":
                automationClient.InsertText(remoteDebuggingUrl, action.Arguments[0]);
                return [$"TEXT_INSERTED {action.LineNumber:000} {action.Arguments[0].Length}"];
            default:
                throw new ScriptExecutionException($"Unknown keyboard action '{action.Name}'.");
        }
    }

    private static IReadOnlyList<string> ExecuteKeyboardShortcut(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        PressShortcut(remoteDebuggingUrl, automationClient, ParseKeyboardChord(action, action.Arguments[0]), PressDelay(action));
        return [$"KEYBOARD_SHORTCUT {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static void PressShortcut(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        IReadOnlyList<string> keys,
        int? delayMilliseconds)
    {
        var modifiers = keys.Take(keys.Count - 1).ToArray();
        foreach (var modifier in modifiers)
        {
            automationClient.KeyDown(remoteDebuggingUrl, modifier);
        }

        if (delayMilliseconds is null)
        {
            automationClient.Press(remoteDebuggingUrl, keys[^1]);
        }
        else
        {
            PressWithDelay(remoteDebuggingUrl, automationClient, keys[^1], delayMilliseconds.Value);
        }

        foreach (var modifier in modifiers.Reverse())
        {
            automationClient.KeyUp(remoteDebuggingUrl, modifier);
        }
    }

    private static void PressWithDelay(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string key,
        int delayMilliseconds)
    {
        automationClient.KeyDown(remoteDebuggingUrl, key);
        if (delayMilliseconds > 0)
        {
            Thread.Sleep(delayMilliseconds);
        }
        automationClient.KeyUp(remoteDebuggingUrl, key);
    }

    private static int? PressDelay(BrowserScriptAction action)
    {
        if (!action.Options.ContainsKey("delay"))
        {
            return null;
        }

        var delay = GetIntOption(action, "delay", required: true);
        return delay >= 0
            ? delay
            : throw new ScriptExecutionException($"{action.Name} option delay= must be zero or greater.");
    }

    private static IReadOnlyList<string> ParseKeyboardChord(BrowserScriptAction action, string chord)
    {
        var keys = chord
            .Split('+', StringSplitOptions.TrimEntries)
            .Select(NormalizeShortcutKey)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToArray();
        if (keys.Length < 2)
        {
            throw new ScriptExecutionException($"{action.Name} expects a key chord such as Control+S.");
        }

        return keys;
    }

    private static string NormalizeShortcutKey(string key) =>
        key.ToLowerInvariant() switch
        {
            "ctrl" => "Control",
            "cmd" or "command" or "meta" => "Meta",
            "option" => "Alt",
            "esc" => "Escape",
            _ => key
        };

    private static bool IsKeyboardChord(string key) =>
        key.Contains('+', StringComparison.Ordinal);
}

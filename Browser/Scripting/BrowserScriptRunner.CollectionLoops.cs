using System.Text.Json;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteForEachJson(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 2, 2);
        var variable = action.Arguments[0];
        var values = ParseJsonArray(action.Arguments[1], action.Name);
        for (var index = 0; index < values.Count; index++)
        {
            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output,
                [(variable, values[index]), ("index", index.ToString())], $"foreachJson {variable} index={index}");
            if (control == "break") break;
        }
    }

    private void ExecuteForEachList(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 2, 2);
        var delimiter = action.Options.GetValueOrDefault("delimiter") ?? ",";
        var includeEmpty = action.Options.TryGetValue("empty", out var empty) && IsLoopTruthy(empty);
        var trimValues = !action.Options.TryGetValue("trim", out var trim) || IsLoopTruthy(trim);
        var values = action.Arguments[1].Split(delimiter)
            .Select(value => trimValues ? value.Trim() : value)
            .Where(value => includeEmpty || value.Length > 0)
            .ToArray();

        for (var index = 0; index < values.Length; index++)
        {
            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output,
                [(action.Arguments[0], values[index]), ("index", index.ToString())], $"foreachList {action.Arguments[0]}={values[index]} index={index}");
            if (control == "break") break;
        }
    }

    private static IReadOnlyList<string> ParseJsonArray(string json, string actionName)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind is not JsonValueKind.Array)
            {
                throw new ScriptExecutionException($"{actionName} expects a JSON array.");
            }

            return document.RootElement.EnumerateArray().Select(JsonValue).ToArray();
        }
        catch (JsonException exception)
        {
            throw new ScriptExecutionException($"{actionName} could not parse JSON array: {exception.Message}");
        }
    }

    private static string JsonValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.ToString(),
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };

    private static bool IsLoopTruthy(string value) =>
        value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("yes", StringComparison.OrdinalIgnoreCase);
}

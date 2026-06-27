namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private bool TryEvaluateInlineActionCondition(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string condition,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        out bool result)
    {
        result = false;
        if (TryEvaluateLogicalActionCondition(remoteDebuggingUrl, automationClient, condition, context, recorder, out result))
        {
            return true;
        }

        var split = SplitActionComparison(condition);
        if (split is not null &&
            TryRunConditionAction(remoteDebuggingUrl, automationClient, split.Value.Left, context, recorder, out var payload, out _, out var succeeded))
        {
            result = succeeded && new ConditionExpression($"{QuoteCondition(payload)} {split.Value.Operator} {split.Value.Right}").Evaluate();
            return true;
        }

        if (!LooksLikeRunnableConditionAction(condition) ||
            !TryRunConditionAction(remoteDebuggingUrl, automationClient, condition, context, recorder, out payload, out var hasPayload, out succeeded))
        {
            return false;
        }

        result = succeeded && (hasPayload ? new ConditionExpression(payload).Evaluate() : true);
        return true;
    }

    private bool TryEvaluateLogicalActionCondition(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string condition,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        out bool result)
    {
        if (SplitTopLevelCondition(condition, "||") is { Count: > 1 } orParts)
        {
            result = orParts.Any(part => EvaluateConditionPart(remoteDebuggingUrl, automationClient, part, context, recorder));
            return true;
        }

        if (SplitTopLevelCondition(condition, "&&") is { Count: > 1 } andParts)
        {
            result = andParts.All(part => EvaluateConditionPart(remoteDebuggingUrl, automationClient, part, context, recorder));
            return true;
        }

        result = false;
        return false;
    }

    private bool EvaluateConditionPart(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string condition,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder) =>
        TryEvaluateInlineActionCondition(remoteDebuggingUrl, automationClient, condition, context, recorder, out var result)
            ? result
            : new ConditionExpression(condition).Evaluate();

    private bool TryRunConditionAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string actionText,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        out string payload,
        out bool hasPayload,
        out bool succeeded)
    {
        payload = string.Empty;
        hasPayload = false;
        succeeded = false;
        var parsed = parser.Parse(actionText);
        if (!parsed.Success || parsed.Actions.Count is not 1)
        {
            return false;
        }

        try
        {
            var action = ExpandVariables(parsed.Actions[0], context);
            var output = ExecuteConditionAction(remoteDebuggingUrl, automationClient, action, context, recorder);
            hasPayload = TryExtractConditionPayload(output, out payload);
            succeeded = true;
            return true;
        }
        catch (ScriptExecutionException exception) when (exception.Message.StartsWith("Unknown action ", StringComparison.Ordinal))
        {
            payload = string.Empty;
            hasPayload = false;
            succeeded = false;
            return false;
        }
        catch (Exception exception) when (exception is ScriptExecutionException or ChromeDevToolsException or ElementNotFoundException)
        {
            payload = string.Empty;
            hasPayload = false;
            succeeded = false;
            return true;
        }
    }

    private IReadOnlyList<string> ExecuteConditionAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder)
    {
        recorder?.BeforeAction(action);
        var output = ExecuteAction(remoteDebuggingUrl, automationClient, action, context, recorder);
        recorder?.AfterAction(action);
        return output;
    }

    private static bool TryExtractConditionPayload(IReadOnlyList<string> output, out string payload)
    {
        var line = output.LastOrDefault(line => !string.IsNullOrWhiteSpace(line));
        payload = string.Empty;
        if (line is null) return false;
        var first = line.IndexOf(' ');
        var second = first < 0 ? -1 : line.IndexOf(' ', first + 1);
        if (second < 0 || second + 1 >= line.Length) return false;
        payload = line[(second + 1)..];
        return true;
    }

    private static bool LooksLikeRunnableConditionAction(string condition)
    {
        var first = condition.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        return first.Length > 0 && !first.StartsWith('!') && !first.Contains('(') && !first.Contains('$');
    }

    private static ActionComparison? SplitActionComparison(string condition)
    {
        foreach (var op in new[] { "==", "!=", ">=", "<=", ">", "<", "contains", "matches", "in" })
        {
            var index = IndexTopLevelConditionOperator(condition, op);
            if (index > 0) return new(condition[..index].Trim(), op, condition[(index + op.Length)..].Trim());
        }

        return null;
    }

    private static int IndexTopLevelConditionOperator(string value, string token)
    {
        for (var index = 0; index <= value.Length - token.Length; index++)
        {
            if (ConditionDepth(value, index) is not 0 || !value.AsSpan(index, token.Length).SequenceEqual(token)) continue;
            if (char.IsLetter(token[0]) && !HasWordBoundaries(value, token, index)) continue;
            return index;
        }
        return -1;
    }

    private static bool HasWordBoundaries(string value, string token, int index)
    {
        var before = index is 0 || char.IsWhiteSpace(value[index - 1]);
        var afterIndex = index + token.Length;
        return before && (afterIndex >= value.Length || char.IsWhiteSpace(value[afterIndex]));
    }

    private static int ConditionDepth(string value, int until)
    {
        var depth = 0;
        var quoted = false;
        for (var index = 0; index < until; index++)
        {
            if (value[index] is '"') quoted = !quoted;
            if (quoted) continue;
            depth += value[index] switch { '(' => 1, ')' => -1, _ => 0 };
        }
        return depth;
    }

    private static List<string> SplitTopLevelCondition(string value, string token)
    {
        var parts = new List<string>();
        var start = 0;
        for (var index = 0; index <= value.Length - token.Length; index++)
        {
            if (ConditionDepth(value, index) is 0 && value.AsSpan(index, token.Length).SequenceEqual(token))
            {
                parts.Add(value[start..index].Trim());
                start = index + token.Length;
            }
        }
        if (parts.Count > 0) parts.Add(value[start..].Trim());
        return parts;
    }

    private static string QuoteCondition(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private readonly record struct ActionComparison(string Left, string Operator, string Right);
}

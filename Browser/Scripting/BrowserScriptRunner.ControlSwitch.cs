namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteSwitch(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException("switch requires case or default blocks.");
        }

        var subject = ReadSwitchSubject(remoteDebuggingUrl, automationClient, action, context, recorder);
        BrowserScriptAction? fallback = null;
        foreach (var child in action.Children)
        {
            if (child.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                RequireDefault(child, ref fallback);
                continue;
            }

            if (!child.Name.Equals("case", StringComparison.OrdinalIgnoreCase))
            {
                throw new ScriptExecutionException("switch can contain only case or default blocks.");
            }

            if (!CaseMatches(subject, child, context))
            {
                continue;
            }

            ExecuteSwitchBranch(remoteDebuggingUrl, automationClient, child, context, recorder, output);
            return;
        }

        if (fallback is not null)
        {
            ExecuteSwitchBranch(remoteDebuggingUrl, automationClient, fallback, context, recorder, output);
        }
    }

    private static void RequireDefault(BrowserScriptAction action, ref BrowserScriptAction? fallback)
    {
        RequireArgumentCount(action, 0, 0);
        if (fallback is not null)
        {
            throw new ScriptExecutionException("switch can have only one default block.");
        }

        fallback = action;
    }

    private string ReadSwitchSubject(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder)
    {
        var subject = StripConditionParens(string.Join(' ', action.Arguments.Select(argument => ExpandVariables(argument, context))));
        return TryRunConditionAction(remoteDebuggingUrl, automationClient, subject, context, recorder, out var payload, out var hasPayload, out var succeeded) && succeeded
            ? hasPayload ? payload : "true"
            : subject;
    }

    private void ExecuteSwitchBranch(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction branch,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output) =>
        WithMacroScope(context, () => ExecuteActions(remoteDebuggingUrl, automationClient, branch.Children, context, recorder, output));

    private static bool CaseMatches(string subject, BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        var op = action.Arguments[0];
        var values = action.Arguments.Skip(1).Select(value => ExpandVariables(value, context)).ToArray();
        if (IsComparisonOperator(op))
        {
            if (values.Length is 0) throw new ScriptExecutionException("case comparison requires a value.");
            return new ConditionExpression($"{QuoteCase(subject)} {op} {QuoteCase(string.Join(' ', values))}").Evaluate();
        }

        if (op.Equals("contains", StringComparison.OrdinalIgnoreCase))
        {
            if (values.Length is 0) throw new ScriptExecutionException("case contains requires a value.");
            return subject.Contains(string.Join(' ', values), StringComparison.Ordinal);
        }

        if (op.Equals("matches", StringComparison.OrdinalIgnoreCase))
        {
            if (values.Length is 0) throw new ScriptExecutionException("case matches requires a regex pattern.");
            return System.Text.RegularExpressions.Regex.IsMatch(subject, string.Join(' ', values));
        }

        if (op.Equals("in", StringComparison.OrdinalIgnoreCase))
        {
            if (values.Length is 0) throw new ScriptExecutionException("case in requires at least one value.");
            return values.Any(value => string.Equals(subject, value, StringComparison.Ordinal));
        }

        var expected = ExpandVariables(string.Join(' ', action.Arguments), context);
        return string.Equals(subject, expected, StringComparison.Ordinal);
    }

    private static bool IsComparisonOperator(string value) => value is "==" or "!=" or ">" or "<" or ">=" or "<=";

    private static string QuoteCase(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

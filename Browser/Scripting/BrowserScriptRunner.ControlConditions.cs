using System.Globalization;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteIf(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        IReadOnlyList<BrowserScriptAction> branches,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        foreach (var branch in branches)
        {
            var matches = branch.Name.Equals("else", StringComparison.OrdinalIgnoreCase) ||
                EvaluateCondition(remoteDebuggingUrl, automationClient, branch, context);
            if (!matches) continue;
            WithMacroScope(context, () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, branch.Children, context, recorder, output));
            return;
        }
    }

    private bool EvaluateCondition(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context)
    {
        if (action.Arguments.Count is 0)
        {
            throw new ScriptExecutionException($"{action.Name} requires a condition.");
        }

        var condition = StripConditionParens(string.Join(' ', action.Arguments.Select(argument => ExpandVariables(argument, context))));
        return LooksLikeActionCondition(condition)
            ? EvaluateActionCondition(remoteDebuggingUrl, automationClient, condition, context)
            : new ConditionExpression(condition).Evaluate();
    }

    private bool EvaluateActionCondition(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string condition,
        ScriptExecutionContext context)
    {
        var parsed = parser.Parse(condition);
        if (!parsed.Success || parsed.Actions.Count is not 1)
        {
            throw new ScriptExecutionException($"Invalid action condition '{condition}'.");
        }

        try
        {
            ExecuteAction(remoteDebuggingUrl, automationClient, ExpandVariables(parsed.Actions[0], context), context, recorder: null);
            return true;
        }
        catch (Exception exception) when (exception is ScriptExecutionException or ChromeDevToolsException or ElementNotFoundException)
        {
            return false;
        }
    }

    private static bool LooksLikeActionCondition(string condition)
    {
        var first = condition.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        return first.Contains("assert", StringComparison.OrdinalIgnoreCase) ||
            first.StartsWith("expect", StringComparison.OrdinalIgnoreCase) ||
            first.StartsWith("toBe", StringComparison.OrdinalIgnoreCase) ||
            first.StartsWith("toHave", StringComparison.OrdinalIgnoreCase) ||
            first.StartsWith("waitFor", StringComparison.OrdinalIgnoreCase) ||
            first.Equals("contains", StringComparison.OrdinalIgnoreCase);
    }

    private static string StripConditionParens(string condition)
    {
        condition = condition.Trim();
        return condition.StartsWith('(') && condition.EndsWith(')')
            ? condition[1..^1].Trim()
            : condition;
    }

    private sealed class ConditionExpression
    {
        private readonly string expression;
        public ConditionExpression(string expression) => this.expression = expression;
        public bool Evaluate() => EvalOr(expression.Trim());

        private static bool EvalOr(string value) => SplitTopLevel(value, "||") is { Count: > 1 } parts
            ? parts.Any(EvalAnd)
            : EvalAnd(value);

        private static bool EvalAnd(string value) => SplitTopLevel(value, "&&") is { Count: > 1 } parts
            ? parts.All(EvalUnary)
            : EvalUnary(value);

        private static bool EvalUnary(string value)
        {
            value = TrimOuter(value.Trim());
            return value.StartsWith('!') ? !EvalUnary(value[1..]) : EvalComparison(value);
        }

        private static bool EvalComparison(string value)
        {
            foreach (var op in new[] { "==", "!=", ">=", "<=", ">", "<" })
            {
                var index = IndexTopLevel(value, op);
                if (index >= 0) return Compare(value[..index], value[(index + op.Length)..], op);
            }

            return Truthy(Unquote(value.Trim()));
        }

        private static bool Compare(string left, string right, string op)
        {
            var l = Unquote(left.Trim());
            var r = Unquote(right.Trim());
            if (double.TryParse(l, NumberStyles.Float, CultureInfo.InvariantCulture, out var ln) &&
                double.TryParse(r, NumberStyles.Float, CultureInfo.InvariantCulture, out var rn))
            {
                return op switch { "==" => ln == rn, "!=" => ln != rn, ">" => ln > rn, "<" => ln < rn, ">=" => ln >= rn, "<=" => ln <= rn, _ => false };
            }

            var compare = string.Compare(l, r, StringComparison.Ordinal);
            return op switch { "==" => compare == 0, "!=" => compare != 0, ">" => compare > 0, "<" => compare < 0, ">=" => compare >= 0, "<=" => compare <= 0, _ => false };
        }

        private static bool Truthy(string value) =>
            value.Length > 0 && !value.Equals("false", StringComparison.OrdinalIgnoreCase) && value is not "0";

        private static string Unquote(string value) =>
            value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;

        private static string TrimOuter(string value) =>
            value.StartsWith('(') && value.EndsWith(')') && IndexTopLevel(value[1..^1], "&&") >= -1 ? value[1..^1].Trim() : value;

        private static List<string> SplitTopLevel(string value, string token)
        {
            var parts = new List<string>();
            var start = 0;
            for (var index = 0; index <= value.Length - token.Length; index++)
            {
                if (Depth(value, index) is 0 && value.AsSpan(index, token.Length).SequenceEqual(token))
                {
                    parts.Add(value[start..index].Trim());
                    start = index + token.Length;
                }
            }
            if (parts.Count > 0) parts.Add(value[start..].Trim());
            return parts;
        }

        private static int IndexTopLevel(string value, string token)
        {
            for (var index = 0; index <= value.Length - token.Length; index++)
            {
                if (Depth(value, index) is 0 && value.AsSpan(index, token.Length).SequenceEqual(token)) return index;
            }
            return -1;
        }

        private static int Depth(string value, int until)
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
    }
}

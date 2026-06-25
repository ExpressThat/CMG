namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static void RegisterMacro(BrowserScriptAction action, ScriptExecutionContext context)
    {
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException("macro requires a block body.");
        }

        RequireArgumentCount(action, 1, int.MaxValue);
        context.Macros[action.Arguments[0]] = action;
    }

    private void ExecuteMacro(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        if (!context.Macros.TryGetValue(action.Arguments[0], out var macro))
        {
            throw new ScriptExecutionException($"Macro '{action.Arguments[0]}' is not defined.");
        }

        var parameterNames = macro.Arguments.Skip(1).ToArray();
        var values = action.Arguments.Skip(1).ToArray();
        if (values.Length != parameterNames.Length)
        {
            throw new ScriptExecutionException($"Macro '{macro.Arguments[0]}' expects {parameterNames.Length} argument(s), got {values.Length}.");
        }

        WithVariables(context, parameterNames.Zip(values), () =>
            ExecuteActions(remoteDebuggingUrl, automationClient, macro.Children, context, recorder, output));
    }

    private static void WithVariables(ScriptExecutionContext context, IEnumerable<(string Key, string Value)> values, Action body)
    {
        var previous = new Dictionary<string, string?>(StringComparer.Ordinal);
        var previousMacros = context.Macros.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in values)
        {
            previous[key] = context.Variables.TryGetValue(key, out var current) ? current : null;
            context.Variables[key] = value;
        }

        try
        {
            body();
        }
        finally
        {
            foreach (var (key, value) in previous)
            {
                if (value is null)
                {
                    context.Variables.Remove(key);
                    continue;
                }

                context.Variables[key] = value;
            }

            RestoreMacros(context, previousMacros);
        }
    }

    private static void WithMacroScope(ScriptExecutionContext context, Action body)
    {
        var previousMacros = context.Macros.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        try
        {
            body();
        }
        finally
        {
            RestoreMacros(context, previousMacros);
        }
    }

    private static void RestoreMacros(ScriptExecutionContext context, Dictionary<string, BrowserScriptAction> previous)
    {
        context.Macros.Clear();
        foreach (var (name, macro) in previous)
        {
            context.Macros[name] = macro;
        }
    }
}

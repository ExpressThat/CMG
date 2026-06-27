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
        context.Macros[action.Arguments[0]] = new ScriptMacro(action, context.CurrentVariableScopeIndex);
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

        var macroAction = macro.Action;
        var parameterNames = macroAction.Arguments.Skip(1).ToArray();
        var values = action.Arguments.Skip(1).ToArray();
        if (values.Length != parameterNames.Length)
        {
            throw new ScriptExecutionException($"Macro '{macroAction.Arguments[0]}' expects {parameterNames.Length} argument(s), got {values.Length}.");
        }

        WithMacroVariables(context, macro, parameterNames.Zip(values), () =>
            context.PushExecutionContext($"macro {macroAction.Arguments[0]}", () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, macroAction.Children, context, recorder, output)));
    }

    private void ExecuteReturn(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        if (action.Children.Count is 0)
        {
            output.AddRange(ReturnValue(action));
            return;
        }

        RequireArgumentCount(action, 0, 0);
        var childOutput = new List<string>();
        ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, childOutput);
        output.AddRange(childOutput);
        output.Add($"RETURN {action.LineNumber:000} {ExtractBlockPayload("return", childOutput)}");
    }

    private static IReadOnlyList<string> ReturnValue(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        return [$"RETURN {action.LineNumber:000} {string.Join(' ', action.Arguments)}"];
    }

    private static void WithMacroVariables(ScriptExecutionContext context, ScriptMacro macro, IEnumerable<(string Key, string Value)> values, Action body)
    {
        var previousMacros = context.Macros.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        try
        {
            var parentScopes = context.CaptureVariableScopes(macro.DefinitionScopeIndex);
            context.WithVariableScopes(parentScopes, () => context.PushVariableScope(values, body));
        }
        finally
        {
            RestoreMacros(context, previousMacros);
        }
    }

    private static void WithVariables(ScriptExecutionContext context, IEnumerable<(string Key, string Value)> values, Action body)
    {
        var previous = new Dictionary<string, string?>(StringComparer.Ordinal);
        var previousMacros = context.Macros.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in values)
        {
            previous[key] = context.TryGetVariable(key, out var current) ? current : null;
            context.SetVariable(key, value);
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
                    context.RemoveLocalVariable(key);
                    continue;
                }

                context.SetVariable(key, value);
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

    private static void RestoreMacros(ScriptExecutionContext context, Dictionary<string, ScriptMacro> previous)
    {
        context.Macros.Clear();
        foreach (var (name, macro) in previous)
        {
            context.Macros[name] = macro;
        }
    }
}

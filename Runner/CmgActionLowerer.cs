namespace CMG.Runner;

public sealed class CmgActionLowerer
{
    public IReadOnlyList<string> Lower(CmgNode action)
    {
        var name = action.Kind.ToLowerInvariant();
        return name switch
        {
            "step" or "gif" => LowerStep(action),
            "caption" => [ToLine("showMessageBar", action.Arguments)],
            "fill" => LowerFill(action),
            "assertvisible" => [ToLine("waitForElement", action.Arguments, action.Options)],
            "wait" => LowerWait(action),
            "expecttext" => [ToLine("assertText", action.Arguments)],
            "setviewport" => [ToLine("setViewport", [], action.Options)],
            "click" or "type" or "clear" or "press" or "hover" or "scrollintoview" or "select" or
            "showmessagebar" or "delay" or "html" or "screenshot" or "screenshotpage" or "asserttext" or
            "evaluate" or "movemouse" or "draganddrop" or "listtabs" or "activatetab" or "closetab" or "set" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            _ => [ToLine("evaluate", [BuildUnsupportedExpression(action.Kind)])]
        };
    }

    private IReadOnlyList<string> LowerStep(CmgNode action)
    {
        var caption = action.Arguments.Count > 0 ? action.Arguments[0] : $"Step at line {action.LineNumber}";
        return [ToLine("showMessageBar", [caption]), .. action.Children.SelectMany(Lower)];
    }

    private static IReadOnlyList<string> LowerFill(CmgNode action)
    {
        if (action.Arguments.Count < 2)
        {
            return [ToLine("evaluate", [BuildUnsupportedExpression("fill requires selector and text")])];
        }

        return [
            ToLine("clear", [action.Arguments[0]]),
            ToLine("type", [action.Arguments[0], action.Arguments[1]])
        ];
    }

    private static IReadOnlyList<string> LowerWait(CmgNode action)
    {
        if (action.Arguments.Count is 1 && int.TryParse(action.Arguments[0], out _))
        {
            return [ToLine("delay", action.Arguments)];
        }

        return action.Arguments.Count > 0 ? [ToLine("waitForElement", action.Arguments, action.Options)] : [];
    }

    private static string ToLine(string action, IReadOnlyList<string> args) => ToLine(action, args, new Dictionary<string, string>());

    private static string ToLine(string action, IReadOnlyList<string> args, IReadOnlyDictionary<string, string> options)
    {
        var parts = new List<string> { action };
        parts.AddRange(args.Select(Quote));
        parts.AddRange(options.Select(option => $"{option.Key}={Quote(option.Value)}"));
        return string.Join(' ', parts);
    }

    private static string BuildUnsupportedExpression(string action) =>
        $"(() => {{ throw new Error({Quote($"CMG action '{action}' is planned but not implemented in this slice.")}); }})()";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

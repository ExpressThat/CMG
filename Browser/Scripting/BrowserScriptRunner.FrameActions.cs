namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteFrameAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        if (action.Name.ToLowerInvariant() is "framewaitforelement" or "framewaitforselector")
        {
            return ExecuteFrameWait(remoteDebuggingUrl, automationClient, action);
        }

        var script = action.Name.ToLowerInvariant() switch
        {
            "frameclick" => FrameElement(action, BrowserFrameScripts.Click),
            "framehover" => FrameElement(action, BrowserFrameScripts.Hover),
            "frametype" => FrameText(action, BrowserFrameScripts.Type),
            "framefill" => FrameText(action, BrowserFrameScripts.Fill),
            "frameasserttext" or "frameexpecttext" or "frametohavetext" or "frametocontaintext" or "framecontains" => FrameAssertText(action),
            "frameevaluate" => FrameEvaluate(action),
            "frametextcontent" or "frameinnertext" or "frameinputvalue" or "framegetattribute" or
            "framecomputedstyle" or "frameproperty" or "framecount" or "framelocatorcount" or
            "frameboundingbox" or "framealltextcontents" or "frameallinnertexts" => FrameGetter(action),
            _ => throw new ScriptExecutionException($"Unknown frame action '{action.Name}'.")
        };

        var result = automationClient.Evaluate(remoteDebuggingUrl, script);
        return FrameOutput(action, result);
    }

    private static string FrameElement(BrowserScriptAction action, Func<string, string, string> build)
    {
        action = NormalizeFrameSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);
        return build(action.Arguments[0], action.Arguments[1]);
    }

    private static string FrameText(BrowserScriptAction action, Func<string, string, string, string> build)
    {
        action = NormalizeFrameSelectorArgument(action);
        RequireArgumentCount(action, 3, 3);
        return build(action.Arguments[0], action.Arguments[1], action.Arguments[2]);
    }

    private static string FrameAssertText(BrowserScriptAction action)
    {
        action = NormalizeFrameSelectorArgument(action);
        RequireArgumentCount(action, 3, 3);
        ValidateTextMatchOptions(action, action.Arguments[2]);
        return BrowserFrameScripts.AssertText(
            action.Arguments[0],
            action.Arguments[1],
            action.Arguments[2],
            EventTextMatchMode(action),
            GetBoolOption(action, "ignoreCase"));
    }

    private static string FrameEvaluate(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        return BrowserFrameScripts.Evaluate(action.Arguments[0], action.Arguments[1]);
    }

    private static string FrameGetter(BrowserScriptAction action)
    {
        action = NormalizeFrameSelectorArgument(action);
        return action.Name.ToLowerInvariant() switch
        {
            "frametextcontent" => FrameGetterScript(action, 2, BrowserFrameScripts.TextGetter, "textContent"),
            "frameinnertext" => FrameGetterScript(action, 2, BrowserFrameScripts.TextGetter, "innerText"),
            "frameinputvalue" => FrameGetterScript(action, 2, BrowserFrameScripts.TextGetter, "value"),
            "framegetattribute" => FrameNamedGetterScript(action, BrowserFrameScripts.Attribute),
            "framecomputedstyle" => FrameNamedGetterScript(action, BrowserFrameScripts.ComputedStyle),
            "frameproperty" => FrameNamedGetterScript(action, BrowserFrameScripts.Property),
            "framecount" or "framelocatorcount" => FrameGetterScript(action, 2, BrowserFrameScripts.Count),
            "frameboundingbox" => FrameGetterScript(action, 2, BrowserFrameScripts.BoundingBox),
            "framealltextcontents" => FrameGetterScript(action, 2, BrowserFrameScripts.AllText, "textContent"),
            "frameallinnertexts" => FrameGetterScript(action, 2, BrowserFrameScripts.AllText, "innerText"),
            _ => throw new ScriptExecutionException($"Unknown frame getter '{action.Name}'.")
        };
    }

    private static string FrameGetterScript(BrowserScriptAction action, int count, Func<string, string, string> build)
    {
        RequireArgumentCount(action, count, count);
        return build(action.Arguments[0], action.Arguments[1]);
    }

    private static string FrameGetterScript(BrowserScriptAction action, int count, Func<string, string, string, string> build, string value)
    {
        RequireArgumentCount(action, count, count);
        return build(action.Arguments[0], action.Arguments[1], value);
    }

    private static string FrameNamedGetterScript(BrowserScriptAction action, Func<string, string, string, string> build)
    {
        RequireArgumentCount(action, 3, 3);
        return build(action.Arguments[0], action.Arguments[1], action.Arguments[2]);
    }

    private static IReadOnlyList<string> FrameOutput(BrowserScriptAction action, string result)
    {
        var label = action.Name.ToLowerInvariant() switch
        {
            "frameevaluate" => "FRAME_EVALUATE",
            "frametextcontent" or "frameinnertext" => "FRAME_TEXT",
            "frameinputvalue" => "FRAME_VALUE",
            "framegetattribute" => "FRAME_ATTRIBUTE",
            "framecomputedstyle" => "FRAME_STYLE",
            "frameproperty" => "FRAME_PROPERTY",
            "framecount" or "framelocatorcount" => "FRAME_COUNT",
            "frameboundingbox" => "FRAME_BOUNDING_BOX",
            "framealltextcontents" or "frameallinnertexts" => "FRAME_TEXTS",
            _ => string.Empty
        };
        return string.IsNullOrEmpty(label) ? [$"FRAME {action.LineNumber:000} {action.Name}"] : [$"{label} {action.LineNumber:000} {result}"];
    }

    private static BrowserScriptAction NormalizeFrameSelectorArgument(BrowserScriptAction action)
    {
        var locator = action.Options.FirstOrDefault(pair => IsLocatorOption(pair.Key));
        if (string.IsNullOrWhiteSpace(locator.Key) || action.Arguments.Count is 0)
        {
            return action;
        }

        var locatorArgument = CMG.Runner.CmgLocatorKeys.Format(locator.Key, locator.Value);
        if (action.Arguments.Count > 1 && action.Arguments[1].Equals(locatorArgument, StringComparison.Ordinal))
        {
            return action;
        }

        return action with { Arguments = [action.Arguments[0], locatorArgument, .. action.Arguments.Skip(1)] };
    }
}

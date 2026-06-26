namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteWithin(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, 1);
        if (!string.IsNullOrWhiteSpace(context.CurrentSelectorScope))
        {
            action = ScopeArgument(action, context.CurrentSelectorScope, 0);
        }

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        context.PushSelectorScope(selector, () => ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
    }

    private static BrowserScriptAction ApplySelectorScope(BrowserScriptAction action, ScriptExecutionContext context)
    {
        var scope = context.CurrentSelectorScope;
        if (string.IsNullOrWhiteSpace(scope) || action.Name.Equals("within", StringComparison.OrdinalIgnoreCase))
        {
            return action;
        }

        var name = action.Name.ToLowerInvariant();
        if (ScopedFirstArgumentActions.Contains(name))
        {
            return ScopeArgument(action, scope, 0);
        }

        if (ScopedTwoArgumentActions.Contains(name))
        {
            return ScopeArgument(ScopeArgument(action, scope, 0), scope, 1);
        }

        return name switch
        {
            "asserttext" or "expecttext" or "tohavetext" or "tocontaintext" or "containstext" or "contains" or
            "waitfortext" or "expectnotext" or "expectnottext" or "notcontains" or "notcontainstext" or
            "tonotcontaintext" or "tohavenotext" or "tohavenottext" => ScopeTextAssertion(action, scope),
            "mousemove" or "mousedown" or "mouseup" or "movemouse" or "wheel" => ScopeMouseLikeTarget(action, scope),
            "foreachselector" when action.Arguments.Count >= 2 => ScopeArgument(action, scope, 1),
            _ => action
        };
    }

    private static BrowserScriptAction ScopeTextAssertion(BrowserScriptAction action, string scope) =>
        action.Arguments.Count switch
        {
            0 => action,
            1 => action with { Arguments = [scope, action.Arguments[0]] },
            _ => ScopeArgument(action, scope, 0)
        };

    private static BrowserScriptAction ScopeMouseLikeTarget(BrowserScriptAction action, string scope)
    {
        if (action.Options.TryGetValue("selector", out var selector))
        {
            var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase)
            {
                ["selector"] = CombineSelector(scope, selector)
            };
            return action with { Options = options };
        }

        return action.Arguments.Count is 1 && !IsViewportAlias(action.Arguments[0])
            ? ScopeArgument(action, scope, 0)
            : action;
    }

    private static BrowserScriptAction ScopeArgument(BrowserScriptAction action, string scope, int index)
    {
        if (index >= action.Arguments.Count)
        {
            return action;
        }

        var arguments = action.Arguments.ToArray();
        arguments[index] = CombineSelector(scope, arguments[index]);
        return action with { Arguments = arguments };
    }

    private static string CombineSelector(string scope, string selector)
    {
        if (string.IsNullOrWhiteSpace(selector) || IsRichLocator(selector))
        {
            return selector;
        }

        return selector.StartsWith("css=", StringComparison.OrdinalIgnoreCase)
            ? $"css={scope} {selector[4..]}"
            : $"{scope} {selector}";
    }

    private static bool IsRichLocator(string selector) =>
        selector.Contains('=', StringComparison.Ordinal) &&
        !selector.StartsWith("css=", StringComparison.OrdinalIgnoreCase);

    private static bool IsViewportAlias(string value) =>
        value.ToLowerInvariant() is "center" or "top" or "bottom" or "left" or "right" or
            "topleft" or "topright" or "bottomleft" or "bottomright";

    private static readonly HashSet<string> ScopedFirstArgumentActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "waitforelement", "waitforselector", "assertvisible", "click", "dblclick", "doubleclick", "rightclick",
        "contextclick", "tap", "touchtap", "presssequentially", "fill", "check", "uncheck", "focus", "blur",
        "selecttext", "dispatchevent", "type", "clear", "hover", "scrollintoview", "select", "selectoption",
        "html", "screenshot", "textcontent", "innertext", "inputvalue", "getattribute", "count", "locatorcount",
        "boundingbox", "alltextcontents", "allinnertexts", "expectvisible", "tobevisible", "waitforvisible",
        "expecthidden", "tobehidden", "waitforhidden", "expectenabled", "tobeenabled", "expectdisabled",
        "tobedisabled", "expectattached", "tobeattached", "expectdetached", "tobedetached", "expecteditable",
        "tobeeditable", "expectempty", "tobeempty", "expectfocused", "tobefocused", "expectinviewport",
        "tobeinviewport", "expectvalue", "tohavevalue", "expectvalues", "tohavevalues", "expectattribute",
        "tohaveattribute", "expectclass", "tohaveclass", "expectid", "tohaveid", "expectcss", "tohavecss",
        "expectproperty", "tohavejsproperty", "expectaccessiblename", "tohaveaccessiblename", "expectrole",
        "tohaverole", "expectchecked", "tobechecked", "expectcount", "tohavecount", "evaluateonselector",
        "evalonselector", "evaluateall", "evalall", "uploadfiles", "setinputfiles", "selectfile", "expectscreenshot",
        "tohavescreenshot", "download", "frame", "framelocator"
    };

    private static readonly HashSet<string> ScopedTwoArgumentActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "draganddrop", "dragto"
    };
}

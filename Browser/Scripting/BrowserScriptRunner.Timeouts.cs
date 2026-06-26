namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static BrowserScriptAction ApplyTimeoutDefaults(BrowserScriptAction action, ScriptExecutionContext context)
    {
        if (action.Options.ContainsKey("timeout"))
        {
            return action;
        }

        var timeout = DefaultTimeoutFor(action.Name, context);
        return timeout is null ? action : AddTimeout(action, timeout.Value);
    }

    private static IReadOnlyList<string> ExecuteTimeoutDefaultAction(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = ParsePositiveInt(action.Arguments[0], action.Name);
        var outputName = action.Name.ToLowerInvariant() switch
        {
            "setdefaultnavigationtimeout" => SetNavigationTimeout(context, timeout),
            "setdefaultassertiontimeout" or "setdefaultexpecttimeout" => SetAssertionTimeout(context, timeout),
            _ => SetDefaultTimeout(context, timeout)
        };

        return [$"{outputName} {action.LineNumber:000} {timeout}"];
    }

    private static int? DefaultTimeoutFor(string name, ScriptExecutionContext context)
    {
        var normalized = name.ToLowerInvariant();
        if (NavigationTimeoutActions.Contains(normalized))
        {
            return context.NavigationTimeout ?? context.DefaultTimeout;
        }

        if (AssertionTimeoutActions.Contains(normalized))
        {
            return context.AssertionTimeout ?? context.DefaultTimeout;
        }

        return GeneralTimeoutActions.Contains(normalized) ? context.DefaultTimeout : null;
    }

    private static BrowserScriptAction AddTimeout(BrowserScriptAction action, int timeout)
    {
        var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase)
        {
            ["timeout"] = timeout.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        return action with { Options = options };
    }

    private static string SetDefaultTimeout(ScriptExecutionContext context, int timeout)
    {
        context.DefaultTimeout = timeout;
        return "DEFAULT_TIMEOUT";
    }

    private static string SetNavigationTimeout(ScriptExecutionContext context, int timeout)
    {
        context.NavigationTimeout = timeout;
        return "DEFAULT_NAVIGATION_TIMEOUT";
    }

    private static string SetAssertionTimeout(ScriptExecutionContext context, int timeout)
    {
        context.AssertionTimeout = timeout;
        return "DEFAULT_ASSERTION_TIMEOUT";
    }

    private static readonly HashSet<string> NavigationTimeoutActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "navigate", "goto", "visit", "reload", "goback", "goforward", "waitforurl", "waitfortitle",
        "expecturl", "expecttitle", "tohaveurl", "tohavetitle", "waitforloadstate", "waitfornetworkidle",
        "networkidle", "waitfornavigation"
    };

    private static readonly HashSet<string> AssertionTimeoutActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "asserttext", "expecttext", "tohavetext", "tocontaintext", "containstext", "contains", "waitfortext",
        "expectnotext", "expectnottext", "notcontains", "notcontainstext", "tonotcontaintext", "tohavenotext",
        "tohavenottext", "expecteval", "asserteval", "expectexpression", "assertexpression",
        "expectvisible", "tobevisible", "waitforvisible", "expecthidden", "tobehidden", "waitforhidden",
        "expectenabled", "tobeenabled", "expectdisabled", "tobedisabled", "expectattached", "tobeattached",
        "expectdetached", "tobedetached", "expecteditable", "tobeeditable", "expectempty", "tobeempty",
        "expectfocused", "tobefocused", "expectinviewport", "tobeinviewport", "expectvalue", "tohavevalue",
        "expectnotvisible", "tobenotvisible", "expectnothidden", "tobenothidden", "expectnotenabled",
        "tobenotenabled", "expectnotdisabled", "tobenotdisabled", "expectnotattached", "tobenotattached",
        "expectnotdetached", "tobenotdetached", "expectnoteditable", "tobenoteditable", "expectnotempty",
        "tobenotempty", "expectnotfocused", "tobenotfocused", "expectnotinviewport", "tobenotinviewport",
        "expectnotchecked", "tobenotchecked", "expectunchecked", "tobeunchecked", "unchecked",
        "expectvalues", "tohavevalues", "expectattribute", "tohaveattribute", "expectclass", "tohaveclass",
        "expectid", "tohaveid", "expectcss", "tohavecss", "expectproperty", "tohavejsproperty",
        "expectaccessiblename", "tohaveaccessiblename", "expectrole", "tohaverole", "expectchecked",
        "tobechecked", "expectcount", "tohavecount", "expectaccessible"
    };

    private static readonly HashSet<string> GeneralTimeoutActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "waitforelement", "waitforselector", "waitforfunction", "assertvisible", "download", "waitfordownload",
        "waitforconsole", "expectnoconsole", "tohavenoconsole", "waitfordialog", "waitforevent",
        "waitforpageerror", "expectnopageerror", "tohavenopageerror", "waitforrequest", "waitforrequestfinished",
        "waitforrequestfailed", "waitforresponse", "waitforwebsocket", "waitforwebsocketmessage",
        "framewaitforelement", "framewaitforselector", "frameasserttext", "frameexpecttext", "frametohavetext",
        "frametocontaintext", "framecontains", "waitforworker", "waitfortab", "waitforpopup", "apirequest"
    };
}

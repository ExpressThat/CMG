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
            "assertvisible" => LowerSelectorCommand("waitForElement", action),
            "wait" => LowerWait(action),
            "expecttext" => [ToLine("assertText", action.Arguments, action.Options)],
            "expecturl" or "expecttitle" => [ToLine(action.Kind, action.Arguments, action.Options)],
            "expectvisible" or "expecthidden" or "expectenabled" or "expectdisabled" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "expectvalue" => CmgExpectationScripts.Element(action, "value"),
            "expectattribute" => CmgExpectationScripts.Element(action, "attribute"),
            "expectchecked" => CmgExpectationScripts.Element(action, "checked"),
            "expectcount" => CmgExpectationScripts.Element(action, "count"),
            "check" => ElementScript(action, "element.checked = true; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;"),
            "uncheck" => ElementScript(action, "element.checked = false; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;"),
            "focus" => ElementScript(action, "element.focus({ preventScroll: true }); return true;"),
            "blur" => ElementScript(action, "element.blur(); return true;"),
            "selecttext" => ElementScript(action, "element.focus({ preventScroll: true }); element.select?.(); return true;"),
            "dblclick" => LowerMouseEvent(action, "dblclick", button: 0),
            "rightclick" => LowerMouseEvent(action, "contextmenu", button: 2),
            "reload" or "goback" or "goforward" or "waitforurl" or "waitforloadstate" or "waitfornavigation" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "waitforselector" or "waitforfunction" or "waitfortimeout" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "localstorage" or "sessionstorage" or "cookie" => [ToLine(action.Kind, action.Arguments, action.Options)],
            "setviewport" => [ToLine("setViewport", [], action.Options)],
            "click" or "type" or "clear" or "hover" or "scrollintoview" or "select" or "selectoption" or "html" or "screenshot" or "asserttext" =>
                LowerSelectorCommand(action.Kind, action),
            "navigate" or "waitforelement" or
            "press" or "keydown" or "keyup" or "inserttext" or "showmessagebar" or "delay" or "screenshotpage" or
            "emulate" or "setgeolocation" or "grantpermissions" or "clearpermissions" or "waitfordownload" or
            "captureconsole" or "waitforconsole" or "capturedialogs" or "setdialogbehavior" or "waitfordialog" or
            "waitforevent" or
            "capturepageerrors" or "waitforpageerror" or
            "route" or "mockresponse" or "intercept" or "clearroutes" or "waitforrequest" or "waitforrequestfinished" or "waitforrequestfailed" or "waitforresponse" or "exporthar" or "replayhar" or
            "setextrahttpheaders" or "setheaders" or "clearextrahttpheaders" or "clearheaders" or "setoffline" or
            "frameclick" or "frametype" or "framefill" or "framehover" or "framewaitforelement" or "frameasserttext" or "frameevaluate" or
            "clock" or "tick" or "restoreclock" or
            "clearcontext" or "resetcontext" or
            "newcontext" or "usecontext" or "listcontexts" or "closecontext" or
            "listworkers" or "waitforworker" or "workerevaluate" or "workerintercept" or
            "startcoverage" or "stopcoverage" or
            "accessibilitysnapshot" or "expectaccessible" or
            "readfile" or "fixture" or "writefile" or "appendfile" or "expectfile" or
            "printpdf" or "pdf" or
            "addinitscript" or "evaluateonnewdocument" or "addscripttag" or "addstyletag" or "url" or "title" or "content" or "setcontent" or
            "evaluate" or "dispatchevent" or "movemouse" or "mousemove" or "mousedown" or "mouseup" or "draganddrop" or "listtabs" or "activatetab" or "closetab" or "set" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "download" => LowerSelectorCommand(action.Kind, action),
            "opentab" or "waitfortab" or "waitforpopup" => [ToLine(action.Kind, action.Arguments, action.Options)],
            "apirequest" => [ToLine(action.Kind, action.Arguments, action.Options)],
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

        var resolved = CmgLocator.Resolve(action.Arguments[0], action.LineNumber);
        return [
            .. resolved.PrefixLines,
            ToLine("clear", [resolved.Selector]),
            ToLine("type", [resolved.Selector, action.Arguments[1]])
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

    private static IReadOnlyList<string> LowerSelectorCommand(string command, CmgNode action)
    {
        if (action.Arguments.Count is 0)
        {
            return [ToLine(command, action.Arguments, action.Options)];
        }

        var resolved = CmgLocator.Resolve(action.Arguments[0], action.LineNumber);
        return [
            .. resolved.PrefixLines,
            CmgActionabilityScripts.WaitForActionable(resolved.Selector, action),
            ToLine(command, [resolved.Selector, .. action.Arguments.Skip(1)], action.Options)
        ];
    }

    private static IReadOnlyList<string> ElementScript(CmgNode action, string body)
    {
        var resolved = action.Arguments.Count > 0 ? CmgLocator.Resolve(action.Arguments[0], action.LineNumber) : new CmgResolvedLocator(string.Empty, []);
        return [
            .. resolved.PrefixLines,
            CmgActionabilityScripts.WaitForActionable(resolved.Selector, action),
            ToLine("evaluate", [$"(() => {{ const element = document.querySelector({QuoteJs(resolved.Selector)}); if (!element) throw new Error('No element matched selector {resolved.Selector}'); {body} }})()"])
        ];
    }

    private static IReadOnlyList<string> LowerMouseEvent(CmgNode action, string eventName, int button)
    {
        var resolved = action.Arguments.Count > 0 ? CmgLocator.Resolve(action.Arguments[0], action.LineNumber) : new CmgResolvedLocator(string.Empty, []);
        var selector = resolved.Selector;
        var buttons = button == 2 ? 2 : 1;
        var expression = $"(() => {{ const element = document.querySelector({QuoteJs(selector)}); if (!element) throw new Error('No element matched selector {selector}'); const rect = element.getBoundingClientRect(); const options = {{ bubbles: true, cancelable: true, button: {button}, buttons: {buttons}, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }}; element.dispatchEvent(new MouseEvent('{eventName}', options)); return true; }})()";
        return [
            .. resolved.PrefixLines,
            CmgActionabilityScripts.WaitForActionable(selector, action),
            ToLine("hover", [selector]),
            ToLine("evaluate", [expression])
        ];
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

    private static string QuoteJs(string value) => Quote(value);
}

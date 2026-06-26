namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    public IReadOnlyList<string> Lower(CmgNode action)
    {
        var name = action.Kind.ToLowerInvariant();
        return name switch
        {
            "step" or "gif" or "recordvideo" or "screencast" => LowerStep(action),
            "macro" or "if" or "elseif" or "else" or "for" or "foreach" or "foreachselector" or
            "while" or "repeat" or "retry" or "try" or "catch" or "finally" or "switch" or "case" or "default" => LowerControlBlock(action),
            "call" or "return" or "break" or "continue" => [ToLine(action.Kind, action.Arguments, action.Options)],
            "caption" => [ToLine("showMessageBar", action.Arguments)],
            "fill" => LowerFill(action),
            "assertvisible" => LowerSelectorCommand("waitForElement", action),
            "wait" => LowerWait(action),
            "expecttext" or "tohavetext" or "tocontaintext" or "containstext" or "waitfortext" or "contains" => LowerTextAssertion(action),
            "expecturl" or "expecttitle" or "tohaveurl" or "tohavetitle" => [ToLine(ToNavigationExpectationName(name), action.Arguments, action.Options)],
            "expectvisible" or "tobevisible" or "waitforvisible" or "expecthidden" or "tobehidden" or "waitforhidden" or
            "expectenabled" or "tobeenabled" or "expectdisabled" or "tobedisabled" =>
                [ToLine(ToExpectationName(name), action.Arguments, action.Options)],
            "expectvalue" or "tohavevalue" => CmgExpectationScripts.Element(action with { Kind = ToExpectationName(name) }, "value"),
            "expectattribute" or "tohaveattribute" => CmgExpectationScripts.Element(action with { Kind = ToExpectationName(name) }, "attribute"),
            "expectchecked" or "tobechecked" => CmgExpectationScripts.Element(action with { Kind = ToExpectationName(name) }, "checked"),
            "expectcount" or "tohavecount" => CmgExpectationScripts.Element(action with { Kind = ToExpectationName(name) }, "count"),
            "check" => ElementScript(action, "element.checked = true; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;"),
            "uncheck" => ElementScript(action, "element.checked = false; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;"),
            "focus" => ElementScript(action, "element.focus({ preventScroll: true }); return true;"),
            "blur" => ElementScript(action, "element.blur(); return true;"),
            "selecttext" => ElementScript(action, "element.focus({ preventScroll: true }); element.select?.(); return true;"),
            "dblclick" or "doubleclick" => LowerMouseEvent(action, "dblclick", button: 0),
            "rightclick" or "contextclick" => LowerMouseEvent(action, "contextmenu", button: 2),
            "tap" or "touchtap" => LowerSelectorCommand(action.Kind, action),
            "reload" or "goback" or "goforward" or "waitforurl" or "waitforloadstate" or "waitfornavigation" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "waitforselector" or "waitforfunction" or "waitfortimeout" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "localstorage" or "sessionstorage" or "cookie" => [ToLine(action.Kind, action.Arguments, action.Options)],
            "setviewport" => [ToLine("setViewport", action.Arguments, action.Options)],
            "viewport" or "setviewportsize" => [LowerViewportAlias(action)],
            "click" or "type" or "presssequentially" or "clear" or "hover" or "scrollintoview" or "select" or "selectoption" or "html" or "screenshot" or "asserttext" =>
                LowerSelectorCommand(action.Kind, action),
            "goto" or "visit" => [ToLine("navigate", action.Arguments, action.Options)],
            "navigate" or "waitforelement" or
            "press" or "keydown" or "keyup" or "inserttext" or "showmessagebar" or "delay" or "screenshotpage" or
            "setclipboard" or "writeclipboard" or "readclipboard" or "clearclipboard" or
            "emulate" or "setgeolocation" or "grantpermissions" or "clearpermissions" or
            "setjavascriptenabled" or "javascriptenabled" or "bypasscsp" or "serviceworkers" or "setserviceworkers" or "waitfordownload" or
            "captureconsole" or "waitforconsole" or "capturedialogs" or
            "setdialogbehavior" or "ondialog" or "handledialog" or "dialogbehavior" or "waitfordialog" or
            "waitforevent" or
            "capturepageerrors" or "waitforpageerror" or
            "route" or "mockresponse" or "intercept" or "clearroutes" or "waitforrequest" or "waitforrequestfinished" or "waitforrequestfailed" or "waitforresponse" or "exporthar" or "replayhar" or
            "routewebsocket" or "clearwebsocketroutes" or "waitforwebsocket" or "waitforwebsocketmessage" or
            "setextrahttpheaders" or "setheaders" or "clearextrahttpheaders" or "clearheaders" or
            "sethttpcredentials" or "httpcredentials" or "authenticate" or "clearhttpcredentials" or
            "setproxy" or "proxy" or "clearproxy" or "setoffline" or
            "frameclick" or "frametype" or "framefill" or "framehover" or "framewaitforelement" or "frameasserttext" or "frameevaluate" or
            "clock" or "tick" or "restoreclock" or
            "clearcontext" or "resetcontext" or
            "newcontext" or "usecontext" or "listcontexts" or "closecontext" or
            "listworkers" or "waitforworker" or "workerevaluate" or "workerintercept" or
            "startcoverage" or "stopcoverage" or
            "accessibilitysnapshot" or "expectaccessible" or
            "readfile" or "fixture" or "writefile" or "appendfile" or "expectfile" or
            "printpdf" or "pdf" or
            "addinitscript" or "evaluateonnewdocument" or "addscripttag" or "addstyletag" or "exposefunction" or "exposebinding" or "url" or "title" or "content" or "setcontent" or
            "textcontent" or "innertext" or "inputvalue" or "getattribute" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "set" => LowerSet(action),
            "evaluate" or "expecteval" or "asserteval" or "expectexpression" or "assertexpression" or
            "evaluateonselector" or "evalonselector" or "evaluateall" or "evalall" or
            "dispatchevent" or "movemouse" or "mousemove" or "mousedown" or "mouseup" or
            "scrollto" or "scrollby" or "wheel" or "draganddrop" or "listtabs" or "activatetab" or "closetab" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "dragto" => [ToLine("dragAndDrop", action.Arguments, action.Options)],
            "download" => LowerSelectorCommand(action.Kind, action),
            "setinputfiles" or "selectfile" => [ToLine("uploadFiles", action.Arguments, action.Options)],
            "tohavescreenshot" => [ToLine("expectScreenshot", action.Arguments, action.Options)],
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

    private IReadOnlyList<string> LowerSet(CmgNode action)
    {
        if (action.Children.Count is 0)
        {
            return [ToLine("set", action.Arguments, action.Options)];
        }

        return [
            ToLine("set", action.Arguments, action.Options) + " {",
            .. action.Children.SelectMany(Lower),
            "}"
        ];
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

    private static IReadOnlyList<string> LowerTextAssertion(CmgNode action)
    {
        if (action.Arguments.Count is 1 &&
            (action.Kind.Equals("contains", StringComparison.OrdinalIgnoreCase) ||
            action.Kind.Equals("toContainText", StringComparison.OrdinalIgnoreCase)))
        {
            return [ToLine("assertText", ["body", action.Arguments[0]], action.Options)];
        }

        if (action.Arguments.Count is 0)
        {
            return [ToLine("assertText", action.Arguments, action.Options)];
        }

        var resolved = CmgLocator.Resolve(action.Arguments[0], action.LineNumber);
        return [
            .. resolved.PrefixLines,
            ToLine("assertText", [resolved.Selector, .. action.Arguments.Skip(1)], action.Options)
        ];
    }

    private static string LowerViewportAlias(CmgNode action)
    {
        if (action.Arguments.Count is 0)
        {
            return ToLine("setViewport", [], action.Options);
        }

        if (action.Arguments.Count is 2 &&
            !action.Options.ContainsKey("width") &&
            !action.Options.ContainsKey("height"))
        {
            var options = new Dictionary<string, string>(action.Options)
            {
                ["width"] = action.Arguments[0],
                ["height"] = action.Arguments[1]
            };
            return ToLine("setViewport", [], options);
        }

        return ToLine("setViewport", action.Arguments, action.Options);
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
        $"(() => {{ throw new Error({Quote($"Unsupported CMG action '{action}'. See docs/scripting/actions.md for supported actions.")}); }})()";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)}\"";
    private static string QuoteJs(string value) => Quote(value);
}

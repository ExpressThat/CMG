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
            "expecturl" => [ToLine("evaluate", [BuildExpectUrl(action)])],
            "expecttitle" => [ToLine("evaluate", [BuildExpectTitle(action)])],
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
            "reload" => [ToLine("evaluate", ["location.reload()"])],
            "goback" => [ToLine("evaluate", ["history.back()"])],
            "goforward" => [ToLine("evaluate", ["history.forward()"])],
            "waitforurl" => [ToLine("evaluate", [BuildWaitForUrl(action)])],
            "localstorage" => [ToLine("evaluate", [BuildStorage(action, "localStorage")])],
            "sessionstorage" => [ToLine("evaluate", [BuildStorage(action, "sessionStorage")])],
            "cookie" => [ToLine("evaluate", [BuildCookie(action)])],
            "setviewport" => [ToLine("setViewport", [], action.Options)],
            "click" or "type" or "clear" or "hover" or "scrollintoview" or "select" or "html" or "screenshot" or "asserttext" =>
                LowerSelectorCommand(action.Kind, action),
            "navigate" or "waitforelement" or
            "press" or "showmessagebar" or "delay" or "screenshotpage" or "emulate" or "waitfordownload" or
            "captureconsole" or "waitforconsole" or "capturepageerrors" or "waitforpageerror" or
            "route" or "mockresponse" or "intercept" or "clearroutes" or "waitforresponse" or "exporthar" or "replayhar" or
            "frameclick" or "frametype" or "framefill" or "framehover" or "framewaitforelement" or "frameasserttext" or "frameevaluate" or
            "clock" or "tick" or "restoreclock" or
            "clearcontext" or "resetcontext" or
            "newcontext" or "usecontext" or "listcontexts" or "closecontext" or
            "listworkers" or "waitforworker" or "workerevaluate" or "workerintercept" or
            "startcoverage" or "stopcoverage" or
            "accessibilitysnapshot" or "expectaccessible" or
            "readfile" or "fixture" or "writefile" or "appendfile" or "expectfile" or
            "printpdf" or "pdf" or
            "addinitscript" or "evaluateonnewdocument" or
            "evaluate" or "movemouse" or "draganddrop" or "listtabs" or "activatetab" or "closetab" or "set" =>
                [ToLine(action.Kind, action.Arguments, action.Options)],
            "download" => LowerSelectorCommand(action.Kind, action),
            "opentab" or "waitfortab" => [ToLine(action.Kind, action.Arguments, action.Options)],
            "apirequest" => [],
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
        var expression = $"(() => {{ const element = document.querySelector({QuoteJs(selector)}); if (!element) throw new Error('No element matched selector {selector}'); const rect = element.getBoundingClientRect(); const options = {{ bubbles: true, cancelable: true, button: {button}, buttons: {button + 1}, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }}; element.dispatchEvent(new MouseEvent('{eventName}', options)); return true; }})()";
        return [
            .. resolved.PrefixLines,
            CmgActionabilityScripts.WaitForActionable(selector, action),
            ToLine("hover", [selector]),
            ToLine("evaluate", [expression])
        ];
    }

    private static string BuildExpectUrl(CmgNode action)
    {
        var expected = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        return $"(() => {{ if (!location.href.includes({QuoteJs(expected)})) throw new Error(`Expected URL to contain {expected}, got ${{location.href}}`); return location.href; }})()";
    }

    private static string BuildExpectTitle(CmgNode action)
    {
        var expected = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        return $"(() => {{ if (!document.title.includes({QuoteJs(expected)})) throw new Error(`Expected title to contain {expected}, got ${{document.title}}`); return document.title; }})()";
    }

    private static string BuildWaitForUrl(CmgNode action)
    {
        var expected = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var timeout = action.Options.TryGetValue("timeout", out var value) && int.TryParse(value, out var parsed) ? parsed : 5000;
        return $$"""
        new Promise((resolve, reject) => {
          const expected = {{QuoteJs(expected)}};
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            if (location.href.includes(expected)) {
              resolve(location.href);
              return;
            }

            if (Date.now() >= deadline) {
              reject(new Error(`URL did not match ${expected} within {{timeout}}ms. Last URL: ${location.href}`));
              return;
            }

            setTimeout(poll, 50);
          };
          poll();
        })
        """;
    }

    private static string BuildStorage(CmgNode action, string storage)
    {
        var operation = action.Arguments.Count > 0 ? action.Arguments[0].ToLowerInvariant() : "get";
        var key = action.Arguments.Count > 1 ? action.Arguments[1] : string.Empty;
        var value = action.Arguments.Count > 2 ? action.Arguments[2] : string.Empty;
        return operation switch
        {
            "set" => $"{storage}.setItem({QuoteJs(key)}, {QuoteJs(value)})",
            "remove" => $"{storage}.removeItem({QuoteJs(key)})",
            "clear" => $"{storage}.clear()",
            _ => $"{storage}.getItem({QuoteJs(key)})"
        };
    }

    private static string BuildCookie(CmgNode action)
    {
        if (action.Arguments.Count >= 2 && action.Arguments[0].Equals("set", StringComparison.OrdinalIgnoreCase))
        {
            return $"document.cookie = {QuoteJs($"{action.Arguments[1]}={action.Arguments.ElementAtOrDefault(2) ?? string.Empty}")}";
        }

        return "document.cookie";
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

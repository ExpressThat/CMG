namespace CMG.Runner;

public static class CmgExpectationScripts
{
    public static IReadOnlyList<string> Element(CmgNode action, string mode)
    {
        if (action.Arguments.Count < RequiredArgumentCount(mode))
        {
            return [Line(Fail($"{action.Kind} received too few arguments."))];
        }

        var resolved = CmgLocator.Resolve(action.Arguments[0], action.LineNumber);
        return [.. resolved.PrefixLines, Line(Build(action, mode, resolved.Selector))];
    }

    private static string Build(CmgNode action, string mode, string selector)
    {
        var timeout = Timeout(action);
        if (mode.Equals("count", StringComparison.Ordinal))
        {
            return BuildCount(action, selector, timeout);
        }

        var body = mode switch
        {
            "value" => $"return element.value?.includes({QuoteJs(action.Arguments[1])}) ? null : `Expected value to contain {action.Arguments[1]}, got ${{element.value ?? ''}}.`;",
            "attribute" => $"const actual = element.getAttribute({QuoteJs(action.Arguments[1])}); return actual?.includes({QuoteJs(action.Arguments[2])}) ? null : `Expected attribute {action.Arguments[1]} to contain {action.Arguments[2]}, got ${{actual}}.`;",
            _ => $"return Boolean(element.checked) === {ExpectedChecked(action).ToString().ToLowerInvariant()} ? null : `Expected checked to be {ExpectedChecked(action).ToString().ToLowerInvariant()}, got ${{Boolean(element.checked)}}.`;"
        };

        return $$"""
        new Promise((resolve, reject) => {
          const selector = {{QuoteJs(selector)}};
          const deadline = Date.now() + {{timeout}};
          const check = () => {
            const element = document.querySelector(selector);
            if (!element) return `No element matched selector ${selector}.`;
            {{body}}
          };
          const poll = () => {
            const error = check();
            if (!error) { resolve(true); return; }
            if (Date.now() >= deadline) { reject(new Error(error)); return; }
            setTimeout(poll, 50);
          };
          poll();
        })
        """;
    }

    private static string BuildCount(CmgNode action, string selector, int timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const selector = {{QuoteJs(selector)}};
          const expected = {{ExpectedCount(action)}};
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const actual = document.querySelectorAll(selector).length;
            if (actual === expected) { resolve(true); return; }
            if (Date.now() >= deadline) { reject(new Error(`Expected ${expected} elements for ${selector}, got ${actual}.`)); return; }
            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    private static int RequiredArgumentCount(string mode) => mode switch
    {
        "attribute" => 3,
        "checked" => 1,
        _ => 2
    };

    private static bool ExpectedChecked(CmgNode action) =>
        action.Arguments.Count < 2 || !action.Arguments[1].Equals("false", StringComparison.OrdinalIgnoreCase);

    private static int ExpectedCount(CmgNode action) =>
        int.TryParse(action.Arguments[1], out var count) ? count : -1;

    private static int Timeout(CmgNode action) =>
        action.Options.TryGetValue("timeout", out var value) && int.TryParse(value, out var timeout) ? timeout : 0;

    private static string Fail(string message) =>
        $"(() => {{ throw new Error({QuoteJs(message)}); }})()";

    private static string Line(string expression) => $"evaluate {Quote(expression)}";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string QuoteJs(string value) => Quote(value);
}

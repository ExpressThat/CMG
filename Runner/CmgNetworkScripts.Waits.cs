namespace CMG.Runner;

public static partial class CmgNetworkScripts
{
    public static string WaitForResponse(CmgNode action) =>
        WaitForNetworkEntry(action, "__cmgResponses", "response");

    public static string WaitForRequestFinished(CmgNode action) =>
        WaitForNetworkEntry(action, "__cmgResponses", "finished request");

    public static string WaitForRequest(CmgNode action) =>
        WaitForNetworkEntry(action, "__cmgRequests", "request");

    public static string WaitForRequestFailed(CmgNode action) =>
        WaitForNetworkEntry(action, "__cmgRequestFailures", "failed request");

    private static string WaitForNetworkEntry(CmgNode action, string storeName, string label)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var timeout = action.Options.GetValueOrDefault("timeout") ?? "5000";
        return InstallPrelude() + $$"""
        new Promise((resolve, reject) => {
          const expected = {
            pattern: {{Quote(pattern)}},
            match: {{Quote(NetworkMatchMode(action))}},
            ignoreCase: {{BoolLiteral(action.Options.GetValueOrDefault("ignoreCase"))}},
            method: {{Quote(action.Options.GetValueOrDefault("method")?.ToUpperInvariant() ?? string.Empty)}},
            status: {{NumberOrNull(action.Options.GetValueOrDefault("status"))}},
            contains: {{Quote(action.Options.GetValueOrDefault("contains") ?? string.Empty)}},
            mocked: {{BoolOrNull(action.Options.GetValueOrDefault("mocked"))}},
            headerName: {{Quote(HeaderName(action))}},
            headerValue: {{Quote(HeaderValue(action))}}
          };
          const normalizeUrl = value => expected.ignoreCase ? String(value || '').toLowerCase() : String(value || '');
          const expectedPattern = normalizeUrl(expected.pattern);
          const expectedRegex = expected.match === 'regex' ? new RegExp(expected.pattern, expected.ignoreCase ? 'i' : '') : null;
          const urlMatches = url => {
            if (expected.match === 'exact') return normalizeUrl(url) === expectedPattern;
            if (expected.match === 'regex') return expectedRegex.test(String(url || ''));
            return normalizeUrl(url).includes(expectedPattern);
          };
          const matches = r =>
            urlMatches(r.url) &&
            (!expected.method || String(r.method || 'GET').toUpperCase() === expected.method) &&
            (expected.status === null || Number(r.status) === expected.status) &&
            (!expected.contains || String(r.body || r.error || '').includes(expected.contains)) &&
            (expected.mocked === null || Boolean(r.mocked) === expected.mocked) &&
            (!expected.headerName || (r.headers && Object.prototype.hasOwnProperty.call(r.headers, expected.headerName) &&
              (!expected.headerValue || String(r.headers[expected.headerName]).includes(expected.headerValue))));
          const deadline = Date.now() + {{timeout}};
          const check = () => {
            const hit = window.{{storeName}}.find(matches);
            if (hit) { resolve(JSON.stringify({ success: true, value: hit })); return; }
            if (Date.now() > deadline) { resolve(JSON.stringify({ success: false, error: '{{TimeoutMessage(label, pattern, action)}}' })); return; }
            setTimeout(check, 50);
          };
          check();
        })
        """;
    }

    private static string TimeoutMessage(string label, string pattern, CmgNode action)
    {
        var filters = new List<string>();
        AddFilter(filters, "method", action.Options.GetValueOrDefault("method")?.ToUpperInvariant());
        AddFilter(filters, "status", action.Options.GetValueOrDefault("status"));
        AddFilter(filters, "contains", action.Options.GetValueOrDefault("contains"));
        AddFilter(filters, "mocked", action.Options.GetValueOrDefault("mocked"));
        AddFilter(filters, "header", HeaderFilter(action));
        AddFilter(filters, "match", NetworkMatchDescription(action));
        var suffix = filters.Count is 0 ? string.Empty : $" with {string.Join(", ", filters)}";
        return EscapeMessage($"Timed out waiting for {label} {pattern}{suffix}");
    }

    private static string NetworkMatchMode(CmgNode action)
    {
        var mode = action.Options.GetValueOrDefault("match") ?? action.Options.GetValueOrDefault("mode");
        return mode?.ToLowerInvariant() is "exact" or "regex" ? mode.ToLowerInvariant() : "contains";
    }

    private static string NetworkMatchDescription(CmgNode action)
    {
        var mode = NetworkMatchMode(action);
        var ignoreCase = BoolLiteral(action.Options.GetValueOrDefault("ignoreCase")) is "true";
        if (mode is "contains" && !ignoreCase)
        {
            return string.Empty;
        }

        return ignoreCase ? $"{mode}, ignoreCase=true" : mode;
    }

    private static string BoolLiteral(string? value) => IsTrue(value) ? "true" : "false";

    private static void AddFilter(List<string> filters, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            filters.Add($"{key}={value}");
        }
    }

    private static string NumberOrNull(string? value) =>
        int.TryParse(value, out var parsed) ? parsed.ToString() : "null";

    private static string BoolOrNull(string? value) =>
        value?.ToLowerInvariant() is "true" or "false" ? value.ToLowerInvariant() : "null";

    private static string HeaderName(CmgNode action)
    {
        var name = action.Options.GetValueOrDefault("headerName");
        if (!string.IsNullOrWhiteSpace(name)) return name.ToLowerInvariant();
        var header = action.Options.GetValueOrDefault("header") ?? string.Empty;
        var index = header.IndexOf(':');
        return (index > 0 ? header[..index] : header).Trim().ToLowerInvariant();
    }

    private static string HeaderValue(CmgNode action)
    {
        var value = action.Options.GetValueOrDefault("headerValue");
        if (!string.IsNullOrWhiteSpace(value)) return value;
        var header = action.Options.GetValueOrDefault("header") ?? string.Empty;
        var index = header.IndexOf(':');
        return index > 0 ? header[(index + 1)..].Trim() : string.Empty;
    }

    private static string HeaderFilter(CmgNode action)
    {
        var name = HeaderName(action);
        var value = HeaderValue(action);
        return string.IsNullOrWhiteSpace(name) ? string.Empty : string.IsNullOrWhiteSpace(value) ? name : $"{name}:{value}";
    }
}

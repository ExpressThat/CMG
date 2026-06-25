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
            method: {{Quote(action.Options.GetValueOrDefault("method")?.ToUpperInvariant() ?? string.Empty)}},
            status: {{NumberOrNull(action.Options.GetValueOrDefault("status"))}},
            contains: {{Quote(action.Options.GetValueOrDefault("contains") ?? string.Empty)}},
            mocked: {{BoolOrNull(action.Options.GetValueOrDefault("mocked"))}}
          };
          const matches = r =>
            r.url.includes(expected.pattern) &&
            (!expected.method || String(r.method || 'GET').toUpperCase() === expected.method) &&
            (expected.status === null || Number(r.status) === expected.status) &&
            (!expected.contains || String(r.body || r.error || '').includes(expected.contains)) &&
            (expected.mocked === null || Boolean(r.mocked) === expected.mocked);
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
        var suffix = filters.Count is 0 ? string.Empty : $" with {string.Join(", ", filters)}";
        return EscapeMessage($"Timed out waiting for {label} {pattern}{suffix}");
    }

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
}

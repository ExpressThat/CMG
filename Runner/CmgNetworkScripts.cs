namespace CMG.Runner;

public static class CmgNetworkScripts
{
    public static string Route(CmgNode action)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var status = action.Options.TryGetValue("status", out var statusValue) ? statusValue : "200";
        var body = action.Options.TryGetValue("body", out var bodyValue) ? bodyValue : string.Empty;
        var contentType = action.Options.TryGetValue("contentType", out var typeValue) ? typeValue : "text/plain";
        return InstallPrelude() + $"window.__cmgRoutes.push({{ pattern: {Quote(pattern)}, status: {status}, body: {Quote(body)}, contentType: {Quote(contentType)} }}); true";
    }

    public static string ClearRoutes() => "window.__cmgRoutes = []; true";

    public static string WaitForResponse(CmgNode action)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var timeout = action.Options.TryGetValue("timeout", out var timeoutValue) ? timeoutValue : "5000";
        return InstallPrelude() + $$"""
        new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          const check = () => {
            const hit = window.__cmgResponses.find(r => r.url.includes({{Quote(pattern)}}));
            if (hit) { resolve(JSON.stringify(hit)); return; }
            if (Date.now() > deadline) { reject(new Error('Timed out waiting for response {{EscapeMessage(pattern)}}')); return; }
            setTimeout(check, 50);
          };
          check();
        })
        """;
    }

    private static string InstallPrelude() =>
        """
        (() => {
          window.__cmgRoutes = window.__cmgRoutes || [];
          window.__cmgResponses = window.__cmgResponses || [];
          if (!window.__cmgFetchPatched) {
            window.__cmgFetchPatched = true;
            const originalFetch = window.fetch.bind(window);
            window.fetch = async (input, init) => {
              const url = typeof input === 'string' ? input : input.url;
              const route = window.__cmgRoutes.find(r => url.includes(r.pattern));
              if (route) {
                const response = new Response(route.body, { status: route.status, headers: { 'content-type': route.contentType } });
                window.__cmgResponses.push({ url, status: route.status, mocked: true });
                return response;
              }
              const response = await originalFetch(input, init);
              window.__cmgResponses.push({ url, status: response.status, mocked: false });
              return response;
            };
          }
        })();
        """;

    private static string Quote(string value) =>
        $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";

    private static string EscapeMessage(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);
}

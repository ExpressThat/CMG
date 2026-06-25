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

    public static string ExportHar() => InstallPrelude() + "JSON.stringify({ log: { version: '1.2', creator: { name: 'CMG', version: '1' }, entries: window.__cmgResponses.map(r => ({ request: { method: r.method || 'GET', url: r.url }, response: { status: r.status, content: { mimeType: r.contentType || 'text/plain', text: r.body || '' } }, _cmgMocked: Boolean(r.mocked), _cmgType: r.type || 'fetch' })) } })";

    public static string ReplayHar(string json) => InstallPrelude() + $$"""
    (() => {
      const har = JSON.parse({{Quote(json)}});
      for (const entry of har.log?.entries || []) {
        const url = entry.request?.url;
        if (!url) continue;
        window.__cmgRoutes.push({
          pattern: url,
          status: Number(entry.response?.status || 200),
          body: entry.response?.content?.text || '',
          contentType: entry.response?.content?.mimeType || 'text/plain'
        });
      }
      return window.__cmgRoutes.length;
    })()
    """;

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
                window.__cmgResponses.push({ method: init?.method || 'GET', url, status: route.status, mocked: true, body: route.body, contentType: route.contentType });
                return response;
              }
              const response = await originalFetch(input, init);
              const body = await response.clone().text().catch(() => '');
              window.__cmgResponses.push({ method: init?.method || 'GET', url, status: response.status, mocked: false, body, contentType: response.headers.get('content-type') || 'text/plain' });
              return response;
            };
          }
          if (!window.__cmgXhrPatched) {
            window.__cmgXhrPatched = true;
            const OriginalXHR = window.XMLHttpRequest;
            window.XMLHttpRequest = function() {
              const xhr = new OriginalXHR();
              let method = 'GET';
              let url = '';
              const open = xhr.open.bind(xhr);
              xhr.open = (...args) => {
                method = String(args[0] || 'GET');
                url = String(args[1] || '');
                return open(...args);
              };
              const send = xhr.send.bind(xhr);
              xhr.send = body => {
                const route = window.__cmgRoutes.find(r => url.includes(r.pattern));
                if (!route) {
                  xhr.addEventListener('loadend', () => window.__cmgResponses.push({ method, url, status: xhr.status, mocked: false, type: 'xhr', body: xhr.responseText || '', contentType: xhr.getResponseHeader('content-type') || 'text/plain' }), { once: true });
                  return send(body);
                }
                window.__cmgResponses.push({ method, url, status: route.status, mocked: true, type: 'xhr', body: route.body, contentType: route.contentType });
                Object.defineProperty(xhr, 'readyState', { configurable: true, get: () => 4 });
                Object.defineProperty(xhr, 'status', { configurable: true, get: () => route.status });
                Object.defineProperty(xhr, 'responseText', { configurable: true, get: () => route.body });
                Object.defineProperty(xhr, 'response', { configurable: true, get: () => route.body });
                setTimeout(() => {
                  xhr.onreadystatechange?.();
                  xhr.onload?.();
                  xhr.dispatchEvent(new Event('readystatechange'));
                  xhr.dispatchEvent(new Event('load'));
                  xhr.dispatchEvent(new Event('loadend'));
                }, 0);
              };
              return xhr;
            };
          }
        })();
        """;

    private static string Quote(string value) =>
        $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";

    private static string EscapeMessage(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);
}

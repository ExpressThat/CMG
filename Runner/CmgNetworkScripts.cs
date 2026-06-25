namespace CMG.Runner;

public static class CmgNetworkScripts
{
    public static string Route(CmgNode action)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var status = action.Options.TryGetValue("status", out var statusValue) ? statusValue : "200";
        var body = action.Options.TryGetValue("body", out var bodyValue) ? bodyValue : string.Empty;
        var contentType = action.Options.TryGetValue("contentType", out var typeValue) ? typeValue : "text/plain";
        var abort = IsAbortRoute(action) ? "true" : "false";
        var error = action.Options.TryGetValue("error", out var errorValue) ? errorValue : "Request aborted by CMG route";
        var method = action.Options.TryGetValue("method", out var methodValue) ? methodValue.ToUpperInvariant() : string.Empty;
        var times = TryReadTimes(action, out var timesValue) ? timesValue.ToString() : "null";
        var delay = action.Options.TryGetValue("delay", out var delayValue) ? delayValue : "0";
        return InstallPrelude() + $"window.__cmgRoutes.push({{ pattern: {Quote(pattern)}, method: {Quote(method)}, times: {times}, delay: {delay}, status: {status}, body: {Quote(body)}, contentType: {Quote(contentType)}, abort: {abort}, error: {Quote(error)} }}); true";
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
            if (hit) { resolve(JSON.stringify({ success: true, value: hit })); return; }
            if (Date.now() > deadline) { resolve(JSON.stringify({ success: false, error: 'Timed out waiting for response {{EscapeMessage(pattern)}}' })); return; }
            setTimeout(check, 50);
          };
          check();
        })
        """;
    }

    public static string WaitForRequestFinished(CmgNode action)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var timeout = action.Options.TryGetValue("timeout", out var timeoutValue) ? timeoutValue : "5000";
        return InstallPrelude() + $$"""
        new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          const check = () => {
            const hit = window.__cmgResponses.find(r => r.url.includes({{Quote(pattern)}}));
            if (hit) { resolve(JSON.stringify({ success: true, value: hit })); return; }
            if (Date.now() > deadline) { resolve(JSON.stringify({ success: false, error: 'Timed out waiting for finished request {{EscapeMessage(pattern)}}' })); return; }
            setTimeout(check, 50);
          };
          check();
        })
        """;
    }

    public static string WaitForRequest(CmgNode action)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var timeout = action.Options.TryGetValue("timeout", out var timeoutValue) ? timeoutValue : "5000";
        return InstallPrelude() + $$"""
        new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          const check = () => {
            const hit = window.__cmgRequests.find(r => r.url.includes({{Quote(pattern)}}));
            if (hit) { resolve(JSON.stringify({ success: true, value: hit })); return; }
            if (Date.now() > deadline) { resolve(JSON.stringify({ success: false, error: 'Timed out waiting for request {{EscapeMessage(pattern)}}' })); return; }
            setTimeout(check, 50);
          };
          check();
        })
        """;
    }

    public static string WaitForRequestFailed(CmgNode action)
    {
        var pattern = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var timeout = action.Options.TryGetValue("timeout", out var timeoutValue) ? timeoutValue : "5000";
        return InstallPrelude() + $$"""
        new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          const check = () => {
            const hit = window.__cmgRequestFailures.find(r => r.url.includes({{Quote(pattern)}}));
            if (hit) { resolve(JSON.stringify({ success: true, value: hit })); return; }
            if (Date.now() > deadline) { resolve(JSON.stringify({ success: false, error: 'Timed out waiting for failed request {{EscapeMessage(pattern)}}' })); return; }
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
          window.__cmgRequests = window.__cmgRequests || [];
          window.__cmgResponses = window.__cmgResponses || [];
          window.__cmgRequestFailures = window.__cmgRequestFailures || [];
          window.__cmgDelay = ms => new Promise(resolve => setTimeout(resolve, Math.max(0, Number(ms) || 0)));
          window.__cmgTakeRoute = (url, method) => {
            const index = window.__cmgRoutes.findIndex(route =>
              url.includes(route.pattern) &&
              (!route.method || route.method === String(method || 'GET').toUpperCase()));
            if (index < 0) return null;
            const route = window.__cmgRoutes[index];
            if (Number.isFinite(route.times)) {
              route.times -= 1;
              if (route.times <= 0) window.__cmgRoutes.splice(index, 1);
            }
            return route;
          };
          if (!window.__cmgFetchPatched) {
            window.__cmgFetchPatched = true;
            const originalFetch = window.fetch.bind(window);
            window.fetch = async (input, init) => {
              const url = typeof input === 'string' ? input : input.url;
              const method = init?.method || input?.method || 'GET';
              window.__cmgRequests.push({ method, url, type: 'fetch', body: init?.body ? String(init.body) : '' });
              const route = window.__cmgTakeRoute(url, method);
              if (route) {
                await window.__cmgDelay(route.delay);
                if (route.abort) {
                  window.__cmgRequestFailures.push({ method, url, type: 'fetch', mocked: true, error: route.error || 'Request aborted by CMG route' });
                  throw new TypeError(route.error || 'Request aborted by CMG route');
                }
                const response = new Response(route.body, { status: route.status, headers: { 'content-type': route.contentType } });
                window.__cmgResponses.push({ method, url, status: route.status, mocked: true, body: route.body, contentType: route.contentType });
                return response;
              }
              try {
                const response = await originalFetch(input, init);
                const body = await response.clone().text().catch(() => '');
                window.__cmgResponses.push({ method, url, status: response.status, mocked: false, body, contentType: response.headers.get('content-type') || 'text/plain' });
                return response;
              } catch (error) {
                window.__cmgRequestFailures.push({ method, url, type: 'fetch', error: String(error?.message || error || 'Request failed') });
                throw error;
              }
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
                window.__cmgRequests.push({ method, url, type: 'xhr', body: body ? String(body) : '' });
                const route = window.__cmgTakeRoute(url, method);
                if (!route) {
                  xhr.addEventListener('loadend', () => window.__cmgResponses.push({ method, url, status: xhr.status, mocked: false, type: 'xhr', body: xhr.responseText || '', contentType: xhr.getResponseHeader('content-type') || 'text/plain' }), { once: true });
                  xhr.addEventListener('error', () => window.__cmgRequestFailures.push({ method, url, type: 'xhr', error: 'Network error' }), { once: true });
                  xhr.addEventListener('abort', () => window.__cmgRequestFailures.push({ method, url, type: 'xhr', error: 'Request aborted' }), { once: true });
                  xhr.addEventListener('timeout', () => window.__cmgRequestFailures.push({ method, url, type: 'xhr', error: 'Request timed out' }), { once: true });
                  return send(body);
                }
                if (route.abort) {
                  Object.defineProperty(xhr, 'readyState', { configurable: true, get: () => 4 });
                  Object.defineProperty(xhr, 'status', { configurable: true, get: () => 0 });
                  setTimeout(() => {
                    window.__cmgRequestFailures.push({ method, url, type: 'xhr', mocked: true, error: route.error || 'Request aborted by CMG route' });
                    xhr.onreadystatechange?.();
                    xhr.onerror?.(new Event('error'));
                    xhr.dispatchEvent(new Event('readystatechange'));
                    xhr.dispatchEvent(new Event('error'));
                    xhr.dispatchEvent(new Event('loadend'));
                  }, Math.max(0, Number(route.delay) || 0));
                  return;
                }
                Object.defineProperty(xhr, 'readyState', { configurable: true, get: () => 4 });
                Object.defineProperty(xhr, 'status', { configurable: true, get: () => route.status });
                Object.defineProperty(xhr, 'responseText', { configurable: true, get: () => route.body });
                Object.defineProperty(xhr, 'response', { configurable: true, get: () => route.body });
                setTimeout(() => {
                  window.__cmgResponses.push({ method, url, status: route.status, mocked: true, type: 'xhr', body: route.body, contentType: route.contentType });
                  xhr.onreadystatechange?.();
                  xhr.onload?.();
                  xhr.dispatchEvent(new Event('readystatechange'));
                  xhr.dispatchEvent(new Event('load'));
                  xhr.dispatchEvent(new Event('loadend'));
                }, Math.max(0, Number(route.delay) || 0));
              };
              return xhr;
            };
          }
        })();
        """;

    private static string Quote(string value) =>
        $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";

    private static string EscapeMessage(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);

    private static bool IsAbortRoute(CmgNode action) =>
        IsTrue(action.Options.GetValueOrDefault("abort")) ||
        string.Equals(action.Options.GetValueOrDefault("action"), "abort", StringComparison.OrdinalIgnoreCase);

    private static bool IsTrue(string? value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static bool TryReadTimes(CmgNode action, out int times)
    {
        times = 0;
        return action.Options.TryGetValue("times", out var value) &&
            int.TryParse(value, out times) &&
            times > 0;
    }
}

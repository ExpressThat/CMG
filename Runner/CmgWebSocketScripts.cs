namespace CMG.Runner;

public static class CmgWebSocketScripts
{
    public static string Route(CmgNode action) => Prelude() + $$"""
      window.__cmgWebSocketRoutes.push({
        pattern: {{Quote(action.Arguments[0])}},
        match: {{Quote(MatchMode(action))}},
        ignoreCase: {{BoolOption(action, "ignoreCase")}},
        message: {{Quote(Option(action, "message"))}},
        close: {{BoolOption(action, "close")}},
        code: {{IntOption(action, "code", 1000)}},
        reason: {{Quote(Option(action, "reason"))}}
      });
      return true;
    })()
    """;

    public static string ClearRoutes() =>
        Prelude() + "window.__cmgWebSocketRoutes = []; return true; })()";

    public static string WaitForSocket(CmgNode action) =>
        Wait(action, "window.__cmgWebSockets", "websocket");

    public static string WaitForMessage(CmgNode action) =>
        Wait(action, "window.__cmgWebSocketMessages", "websocket message");

    private static string Prelude() =>
        """
        (() => {
          window.__cmgWebSocketRoutes ??= [];
          window.__cmgWebSockets ??= [];
          window.__cmgWebSocketMessages ??= [];
          window.__cmgWebSocketMatches = (value, pattern, mode, ignoreCase) => {
            const actual = ignoreCase ? String(value || '').toLowerCase() : String(value || '');
            const expected = ignoreCase ? String(pattern || '').toLowerCase() : String(pattern || '');
            if (mode === 'exact') return actual === expected;
            if (mode === 'regex') return new RegExp(String(pattern || ''), ignoreCase ? 'i' : '').test(String(value || ''));
            return actual.includes(expected);
          };
          if (!window.__cmgOriginalWebSocket) {
            window.__cmgOriginalWebSocket = window.WebSocket;
            window.WebSocket = function(url, protocols) {
              const route = window.__cmgWebSocketRoutes.find(r =>
                window.__cmgWebSocketMatches(String(url), r.pattern, r.match, r.ignoreCase));
              const socket = protocols === undefined
                ? new window.__cmgOriginalWebSocket(url)
                : new window.__cmgOriginalWebSocket(url, protocols);
              const record = { url: String(url), routed: Boolean(route), sent: [] };
              window.__cmgWebSockets.push(record);
              const originalSend = socket.send.bind(socket);
              socket.send = data => { record.sent.push(String(data)); return originalSend(data); };
              socket.addEventListener('message', event => window.__cmgWebSocketMessages.push({
                url: String(url),
                data: String(event.data),
                routed: Boolean(route)
              }));
              if (route) {
                socket.addEventListener('open', () => {
                  if (route.message) {
                    socket.dispatchEvent(new MessageEvent('message', { data: route.message }));
                  }
                  if (route.close) {
                    socket.close(route.code || 1000, route.reason || 'Closed by CMG routeWebSocket');
                  }
                });
              }
              return socket;
            };
            window.WebSocket.prototype = window.__cmgOriginalWebSocket.prototype;
          }
        """;

    private static string Wait(CmgNode action, string collection, string label)
    {
        var timeout = IntOption(action, "timeout", 5_000);
        var pattern = action.Arguments[0];
        var match = MatchMode(action);
        var ignoreCase = BoolOption(action, "ignoreCase");
        return Prelude() + $$"""
          const deadline = Date.now() + {{timeout}};
          const pattern = {{Quote(pattern)}};
          const match = {{Quote(match)}};
          const ignoreCase = {{ignoreCase}};
          return new Promise(resolve => {
            const poll = () => {
              const hit = {{collection}}.find(item =>
                window.__cmgWebSocketMatches(item.url, pattern, match, ignoreCase) ||
                window.__cmgWebSocketMatches(item.data || '', pattern, match, ignoreCase));
              if (hit) { resolve(JSON.stringify({ success: true, value: hit })); return; }
              if (Date.now() > deadline) {
                resolve(JSON.stringify({ success: false, error: 'Timed out waiting for {{label}} {{Escape(pattern)}}' }));
                return;
              }
              setTimeout(poll, 50);
            };
            poll();
          });
        })()
        """;
    }

    private static string Option(CmgNode action, string name) =>
        action.Options.TryGetValue(name, out var value) ? value : string.Empty;

    private static int IntOption(CmgNode action, string name, int defaultValue) =>
        action.Options.TryGetValue(name, out var value) && int.TryParse(value, out var parsed)
            ? parsed
            : defaultValue;

    private static string BoolOption(CmgNode action, string name) =>
        action.Options.TryGetValue(name, out var value) && bool.TryParse(value, out var parsed)
            ? parsed.ToString().ToLowerInvariant()
            : "false";

    private static string MatchMode(CmgNode action)
    {
        var mode = Option(action, "match").ToLowerInvariant();
        return mode is "exact" or "regex" ? mode : "contains";
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Escape(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);
}

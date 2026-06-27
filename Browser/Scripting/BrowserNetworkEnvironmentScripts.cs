namespace CMG.Browser.Scripting;

public static class BrowserNetworkEnvironmentScripts
{
    public static string ExtraHeaders(IReadOnlyDictionary<string, string> headers) =>
        $$"""
        (() => {
          const headers = {{ToObject(headers)}};
          window.__cmgExtraHeaders = headers;
          if (!window.__cmgOriginalFetchForHeaders) {
            window.__cmgOriginalFetchForHeaders = window.fetch.bind(window);
            window.fetch = (input, init = {}) => {
              init.headers = { ...(init.headers || {}), ...(window.__cmgExtraHeaders || {}) };
              return window.__cmgOriginalFetchForHeaders(input, init);
            };
          }
          if (!window.__cmgOriginalXhrOpenForHeaders) {
            window.__cmgOriginalXhrOpenForHeaders = XMLHttpRequest.prototype.open;
            window.__cmgOriginalXhrSendForHeaders = XMLHttpRequest.prototype.send;
            XMLHttpRequest.prototype.send = function(body) {
              for (const [name, value] of Object.entries(window.__cmgExtraHeaders || {})) {
                try { this.setRequestHeader(name, value); } catch { }
              }
              return window.__cmgOriginalXhrSendForHeaders.call(this, body);
            };
          }
          return Object.keys(headers).length.toString();
        })()
        """;

    public static string ClearExtraHeaders() =>
        "(() => { window.__cmgExtraHeaders = {}; return true; })()";

    public static string HttpCredentials(string authorization) =>
        $$"""
        (() => {
          window.__cmgExtraHeaders = { ...(window.__cmgExtraHeaders || {}), Authorization: {{Quote(authorization)}} };
          if (!window.__cmgOriginalFetchForHeaders) {
            window.__cmgOriginalFetchForHeaders = window.fetch.bind(window);
            window.fetch = (input, init = {}) => {
              init.headers = { ...(init.headers || {}), ...(window.__cmgExtraHeaders || {}) };
              return window.__cmgOriginalFetchForHeaders(input, init);
            };
          }
          if (!window.__cmgOriginalXhrSendForHeaders) {
            window.__cmgOriginalXhrSendForHeaders = XMLHttpRequest.prototype.send;
            XMLHttpRequest.prototype.send = function(body) {
              for (const [name, value] of Object.entries(window.__cmgExtraHeaders || {})) {
                try { this.setRequestHeader(name, value); } catch { }
              }
              return window.__cmgOriginalXhrSendForHeaders.call(this, body);
            };
          }
          return true;
        })()
        """;

    public static string ClearHttpCredentials() =>
        "(() => { if (window.__cmgExtraHeaders) delete window.__cmgExtraHeaders.Authorization; return true; })()";

    public static string Proxy(string prefix) =>
        $$"""
        (() => {
          window.__cmgProxyPrefix = {{Quote(prefix)}};
          const rewrite = input => {
            if (!window.__cmgProxyPrefix) return input;
            const url = typeof input === 'string' ? input : input?.url;
            if (!url) return input;
            const target = window.__cmgProxyPrefix + encodeURIComponent(url);
            return typeof input === 'string' ? target : new Request(target, input);
          };
          if (!window.__cmgOriginalFetchForProxy) {
            window.__cmgOriginalFetchForProxy = window.fetch.bind(window);
            window.fetch = (input, init) => window.__cmgOriginalFetchForProxy(rewrite(input), init);
          }
          if (!window.__cmgOriginalXhrOpenForProxy) {
            window.__cmgOriginalXhrOpenForProxy = XMLHttpRequest.prototype.open;
            XMLHttpRequest.prototype.open = function(method, url, ...rest) {
              const target = window.__cmgProxyPrefix ? window.__cmgProxyPrefix + encodeURIComponent(url) : url;
              return window.__cmgOriginalXhrOpenForProxy.call(this, method, target, ...rest);
            };
          }
          return window.__cmgProxyPrefix;
        })()
        """;

    public static string ClearProxy() =>
        "(() => { window.__cmgProxyPrefix = ''; return true; })()";

    public static string Offline(bool offline) =>
        $$"""
        (() => {
          window.__cmgOffline = {{offline.ToString().ToLowerInvariant()}};
          Object.defineProperty(navigator, 'onLine', { configurable: true, get: () => !window.__cmgOffline });
          if (!window.__cmgOriginalFetchForOffline) {
            window.__cmgOriginalFetchForOffline = window.fetch.bind(window);
            window.fetch = (input, init) => window.__cmgOffline
              ? Promise.reject(new TypeError('CMG offline mode is enabled.'))
              : window.__cmgOriginalFetchForOffline(input, init);
          }
          if (!window.__cmgOriginalXhrSendForOffline) {
            window.__cmgOriginalXhrSendForOffline = XMLHttpRequest.prototype.send;
            XMLHttpRequest.prototype.send = function(body) {
              if (window.__cmgOffline) throw new DOMException('CMG offline mode is enabled.', 'NetworkError');
              return window.__cmgOriginalXhrSendForOffline.call(this, body);
            };
          }
          window.dispatchEvent(new Event(window.__cmgOffline ? 'offline' : 'online'));
          return navigator.onLine;
        })()
        """;

    private static string ToObject(IReadOnlyDictionary<string, string> headers) =>
        "{" + string.Join(",", headers.Select(pair => $"{Quote(pair.Key)}:{Quote(pair.Value)}")) + "}";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private static string BuildFirefoxWorkerInterceptScript(WorkerRouteOptions options) => $$"""
    (() => {
      const routes = globalThis.__cmgWorkerRoutes ||= [];
      routes.push({
        pattern: {{BrowserDomScripts.JsonString(options.Pattern)}},
        match: {{BrowserDomScripts.JsonString(options.Match)}},
        ignoreCase: {{options.IgnoreCase.ToString().ToLowerInvariant()}},
        status: {{options.Status}},
        body: {{BrowserDomScripts.JsonString(options.Body)}},
        contentType: {{BrowserDomScripts.JsonString(options.ContentType)}},
        headers: {{FirefoxWorkerHeaders(options)}}
      });
      if (!globalThis.__cmgOriginalFetch) {
        globalThis.__cmgOriginalFetch = globalThis.fetch?.bind(globalThis);
        globalThis.fetch = async (input, init) => {
          const url = typeof input === 'string' ? input : input?.url ?? '';
          const route = routes.find(item => {
            const actual = item.ignoreCase ? String(url).toLowerCase() : String(url);
            const pattern = item.ignoreCase ? String(item.pattern).toLowerCase() : String(item.pattern);
            if (item.match === 'exact') return actual === pattern;
            if (item.match === 'regex') return new RegExp(String(item.pattern), item.ignoreCase ? 'i' : '').test(String(url));
            return actual.includes(pattern);
          });
          return route ? new Response(route.body, { status: route.status, headers: route.headers }) : globalThis.__cmgOriginalFetch(input, init);
        };
      }
      return routes.length.toString();
    })()
    """;

    private static string FirefoxWorkerHeaders(WorkerRouteOptions options)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["content-type"] = options.ContentType
        };
        if (options.Headers is not null)
        {
            foreach (var header in options.Headers)
            {
                headers[header.Key.ToLowerInvariant()] = header.Value;
            }
        }

        return "{" + string.Join(",", headers.Select(header => $"{BrowserDomScripts.JsonString(header.Key)}:{BrowserDomScripts.JsonString(header.Value)}")) + "}";
    }
}

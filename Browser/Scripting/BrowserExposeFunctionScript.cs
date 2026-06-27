namespace CMG.Browser.Scripting;

public static class BrowserExposeFunctionScript
{
    public static string Build(string name, string expression, bool includeSource) =>
        $$"""
        (() => {
          const factory = (() => {
            return ({{expression}});
          })();
          if (typeof factory !== 'function') {
            throw new Error('CMG exposed function {{name}} must evaluate to a function.');
          }
          window[{{Quote(name)}}] = (...args) => {
            const source = {
              page: window,
              frame: window,
              name: {{Quote(name)}}
            };
            return {{(includeSource ? "factory(source, ...args)" : "factory(...args)")}};
          };
          return true;
        })()
        """;

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

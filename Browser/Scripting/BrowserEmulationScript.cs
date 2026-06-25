namespace CMG.Browser.Scripting;

public static class BrowserEmulationScript
{
    public static string Build(IReadOnlyDictionary<string, string> options)
    {
        var lines = new List<string>();
        AddNavigator(lines, options);
        AddMedia(lines, options);
        AddTimezone(lines, options);
        AddGeolocation(lines, options);
        AddPermissions(lines, options);
        return lines.Count is 0 ? string.Empty : $"(() => {{ {string.Join(' ', lines)} return true; }})()";
    }

    public static string BuildGeolocation(double latitude, double longitude, double accuracy) =>
        $"(() => {{ {GeolocationLine(latitude, longitude, accuracy)} return true; }})()";

    public static string BuildPermissions(IReadOnlyList<string> permissions)
    {
        var joined = string.Join(',', permissions);
        return $"(() => {{ {PermissionsLine(joined)} return true; }})()";
    }

    public static string JavaScriptEnabled(bool enabled) =>
        enabled
            ? "(() => { window.__cmgJavaScriptBlocked = false; return true; })()"
            : """
              (() => {
                window.__cmgJavaScriptBlocked = true;
                const originalAppend = Element.prototype.appendChild;
                Element.prototype.appendChild = function(child) {
                  if (window.__cmgJavaScriptBlocked && child?.tagName === 'SCRIPT') {
                    throw new Error('CMG JavaScript blocking rejected a dynamic script.');
                  }
                  return originalAppend.call(this, child);
                };
                window.eval = () => { throw new Error('CMG JavaScript blocking rejected eval.'); };
                window.Function = function() { throw new Error('CMG JavaScript blocking rejected Function.'); };
                return true;
              })()
              """;

    public static string BypassCsp(bool enabled) =>
        $$"""
        (() => {
          window.__cmgBypassCsp = {{enabled.ToString().ToLowerInvariant()}};
          if (window.__cmgBypassCsp) {
            document.querySelectorAll('meta[http-equiv="Content-Security-Policy" i]').forEach(node => node.remove());
          }
          return window.__cmgBypassCsp;
        })()
        """;

    public static string ServiceWorkers(string mode) =>
        $$"""
        (() => {
          window.__cmgServiceWorkers = {{Quote(mode)}};
          if (navigator.serviceWorker && !navigator.serviceWorker.__cmgOriginalRegister) {
            navigator.serviceWorker.__cmgOriginalRegister = navigator.serviceWorker.register.bind(navigator.serviceWorker);
            navigator.serviceWorker.register = (...args) => window.__cmgServiceWorkers === 'block'
              ? Promise.reject(new Error('CMG service worker blocking is enabled.'))
              : navigator.serviceWorker.__cmgOriginalRegister(...args);
          }
          return window.__cmgServiceWorkers;
        })()
        """;

    private static void AddNavigator(List<string> lines, IReadOnlyDictionary<string, string> options)
    {
        if (options.TryGetValue("userAgent", out var userAgent))
        {
            lines.Add($"Object.defineProperty(navigator, 'userAgent', {{ get: () => {Quote(userAgent)}, configurable: true }});");
        }

        if (options.TryGetValue("locale", out var locale))
        {
            lines.Add($"Object.defineProperty(navigator, 'language', {{ get: () => {Quote(locale)}, configurable: true }});");
            lines.Add($"Object.defineProperty(navigator, 'languages', {{ get: () => [{Quote(locale)}], configurable: true }});");
        }
    }

    private static void AddMedia(List<string> lines, IReadOnlyDictionary<string, string> options)
    {
        var hasColor = options.TryGetValue("colorScheme", out var color);
        var hasMotion = options.TryGetValue("reducedMotion", out var motion);
        if (!hasColor && !hasMotion)
        {
            return;
        }

        lines.Add($"const cmgMatchMedia = window.matchMedia.bind(window); window.matchMedia = query => {{ const result = cmgMatchMedia(query); let matches = result.matches; if (query.includes('prefers-color-scheme')) matches = query.includes({Quote(color ?? string.Empty)}); if (query.includes('prefers-reduced-motion')) matches = query.includes({Quote(motion ?? string.Empty)}); return Object.assign(result, {{ matches, media: query }}); }};");
    }

    private static void AddTimezone(List<string> lines, IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("timezone", out var timezone))
        {
            return;
        }

        lines.Add($"const cmgDateTimeFormat = Intl.DateTimeFormat; Intl.DateTimeFormat = function(...args) {{ const formatter = new cmgDateTimeFormat(...args); const original = formatter.resolvedOptions.bind(formatter); formatter.resolvedOptions = () => ({{ ...original(), timeZone: {Quote(timezone)} }}); return formatter; }}; Intl.DateTimeFormat.prototype = cmgDateTimeFormat.prototype;");
    }

    private static void AddGeolocation(List<string> lines, IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("geolocation", out var geolocation))
        {
            return;
        }

        var parts = geolocation.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length is not 2 || !double.TryParse(parts[0], out var lat) || !double.TryParse(parts[1], out var lng))
        {
            throw new ScriptExecutionException("geolocation must be '<latitude>,<longitude>'.");
        }

        lines.Add(GeolocationLine(lat, lng, 1));
    }

    private static void AddPermissions(List<string> lines, IReadOnlyDictionary<string, string> options)
    {
        if (options.TryGetValue("permissions", out var permissions))
        {
            lines.Add(PermissionsLine(permissions));
        }
    }

    private static string GeolocationLine(double lat, double lng, double accuracy) =>
        $"Object.defineProperty(navigator, 'geolocation', {{ configurable: true, value: {{ getCurrentPosition: success => success({{ coords: {{ latitude: {lat}, longitude: {lng}, accuracy: {accuracy} }} }}), watchPosition: success => {{ success({{ coords: {{ latitude: {lat}, longitude: {lng}, accuracy: {accuracy} }} }}); return 1; }}, clearWatch: () => {{ }} }} }});";

    private static string PermissionsLine(string permissions) =>
        $"navigator.permissions.query = descriptor => Promise.resolve({{ name: descriptor.name, state: {Quote(permissions)}.split(',').includes(descriptor.name) ? 'granted' : 'prompt' }});";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

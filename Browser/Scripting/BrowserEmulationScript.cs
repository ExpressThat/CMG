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

    public static string BuildMedia(IReadOnlyDictionary<string, string> options) =>
        $"(() => {{ {MediaLine(options)} return true; }})()";

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
        if (!HasAnyMediaOption(options))
        {
            return;
        }

        lines.Add(MediaLine(options));
    }

    private static bool HasAnyMediaOption(IReadOnlyDictionary<string, string> options) =>
        options.ContainsKey("media") ||
        options.ContainsKey("colorScheme") ||
        options.ContainsKey("reducedMotion") ||
        options.ContainsKey("forcedColors") ||
        options.ContainsKey("contrast");

    private static string MediaLine(IReadOnlyDictionary<string, string> options)
    {
        var media = options.GetValueOrDefault("media", string.Empty);
        var color = options.GetValueOrDefault("colorScheme", string.Empty);
        var motion = options.GetValueOrDefault("reducedMotion", string.Empty);
        var forced = options.GetValueOrDefault("forcedColors", string.Empty);
        var contrast = options.GetValueOrDefault("contrast", string.Empty);
        return $"const cmgMatchMedia = window.__cmgOriginalMatchMedia || window.matchMedia.bind(window); window.__cmgOriginalMatchMedia = cmgMatchMedia; window.__cmgMedia = {{ media: {Quote(media)}, colorScheme: {Quote(color)}, reducedMotion: {Quote(motion)}, forcedColors: {Quote(forced)}, contrast: {Quote(contrast)} }}; window.matchMedia = query => {{ const result = cmgMatchMedia(query); let matches = result.matches; const q = String(query); const state = window.__cmgMedia; if (state.media && (q.includes('screen') || q.includes('print'))) matches = q.includes(state.media); if (state.colorScheme && q.includes('prefers-color-scheme')) matches = q.includes(state.colorScheme); if (state.reducedMotion && q.includes('prefers-reduced-motion')) matches = q.includes(state.reducedMotion); if (state.forcedColors && q.includes('forced-colors')) matches = q.includes(state.forcedColors); if (state.contrast && q.includes('prefers-contrast')) matches = q.includes(state.contrast); return Object.assign(result, {{ matches, media: q }}); }};";
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

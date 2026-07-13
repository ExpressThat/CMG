namespace CMG.Browser.Scripting.Recording;

public sealed record GifDebugOptions(
    bool Enabled = false,
    bool Action = false,
    bool Context = false,
    bool Target = false,
    bool Coordinates = false,
    bool Scroll = false)
{
    public static GifDebugOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string source,
        GifDebugOptions? defaults = null)
    {
        defaults ??= new();
        var enabled = GifRecordingPresetOptions.Boolean(options, "debug", defaults.Enabled, source);
        var reset = options.ContainsKey("debug") && !enabled;
        return new(
            enabled,
            Value(options, "debugAction", !reset && (enabled || defaults.Action), source),
            Value(options, "debugContext", !reset && (enabled || defaults.Context), source),
            Value(options, "debugTarget", !reset && (enabled || defaults.Target), source),
            Value(options, "debugCoordinates", !reset && (enabled || defaults.Coordinates), source),
            Value(options, "debugScroll", !reset && (enabled || defaults.Scroll), source));
    }

    private static bool Value(
        IReadOnlyDictionary<string, string> options,
        string name,
        bool fallback,
        string source) => GifRecordingPresetOptions.Boolean(options, name, fallback, source);
}

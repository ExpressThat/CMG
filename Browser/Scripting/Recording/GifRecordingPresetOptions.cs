namespace CMG.Browser.Scripting.Recording;

public static class GifRecordingPresetOptions
{
    public static bool Boolean(
        IReadOnlyDictionary<string, string> options,
        string name,
        bool fallback,
        string source)
    {
        if (!options.TryGetValue(name, out var value)) return fallback;
        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} option {name}= must be true or false.")
        };
    }
}

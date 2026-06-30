using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Commands;

public static class GifCaptionOptionParser
{
    public static bool TryParse(
        string? style,
        string? position,
        string? severity,
        out BrowserCaptionOptions? options,
        out string error)
    {
        options = null;
        error = string.Empty;
        if (style is null && position is null && severity is null)
        {
            return true;
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(values, "captionStyle", style);
        Add(values, "captionPosition", position);
        Add(values, "captionSeverity", severity);
        try
        {
            options = BrowserCaptionOptions.FromOptions(values, "GIF");
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static void Add(Dictionary<string, string> values, string key, string? value)
    {
        if (value is not null)
        {
            values[key] = value;
        }
    }
}

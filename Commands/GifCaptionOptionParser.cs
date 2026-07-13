using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public static class GifCaptionOptionParser
{
    public static bool TryParse(
        string? style,
        string? position,
        string? severity,
        out BrowserCaptionOptions? options,
        out string error,
        string? size = null,
        bool? autoCaptions = null,
        string? captionTemplate = null)
    {
        options = null;
        error = string.Empty;
        autoCaptions ??= captionTemplate is null ? null : true;
        if (style is null && position is null && severity is null && size is null && autoCaptions is null && captionTemplate is null)
        {
            return true;
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(values, "captionStyle", style);
        Add(values, "captionPosition", position);
        Add(values, "captionSeverity", severity);
        Add(values, "captionSize", size);
        if (autoCaptions is not null) values["autoCaptions"] = autoCaptions.Value ? "true" : "false";
        Add(values, "captionTemplate", captionTemplate);
        try
        {
            ScriptAutoCaption.ValidateTemplate(captionTemplate, "GIF");
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

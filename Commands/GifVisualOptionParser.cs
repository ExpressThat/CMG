using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Commands;

public static class GifVisualOptionParser
{
    public static bool TryParse(
        string? pointerTheme,
        string? pointerColor,
        int? pointerSize,
        string? pointerShadow,
        out PointerVisualOptions visual,
        out string error)
    {
        visual = PointerVisualOptions.Default;
        error = string.Empty;
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(options, "pointerTheme", pointerTheme);
        Add(options, "pointerColor", pointerColor);
        if (pointerSize is not null)
        {
            options["pointerSize"] = pointerSize.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        Add(options, "pointerShadow", pointerShadow);
        try
        {
            visual = PointerVisualOptions.FromOptions(options, "gif");
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message.Replace("gif option", "GIF option", StringComparison.Ordinal);
            return false;
        }
    }

    private static void Add(IDictionary<string, string> options, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            options[name] = value;
        }
    }
}

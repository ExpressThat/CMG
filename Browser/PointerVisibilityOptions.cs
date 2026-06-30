using CMG.Browser.Scripting;

namespace CMG.Browser;

public static class PointerVisibilityOptions
{
    public const string Values = "true, false, auto";

    public static PointerVisibility Parse(string value, string source)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "" => throw new ScriptExecutionException($"{source} showPointer= cannot be empty."),
            "auto" => PointerVisibility.Auto,
            "true" or "yes" or "on" or "1" or "visible" or "show" => PointerVisibility.Visible,
            "false" or "no" or "off" or "0" or "hidden" or "hide" => PointerVisibility.Hidden,
            _ => throw new ScriptExecutionException($"{source} showPointer= must be one of: {Values}.")
        };
    }

    public static bool TryParse(string? value, out PointerVisibility visibility, out string? error)
    {
        visibility = PointerVisibility.Auto;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        try
        {
            visibility = Parse(value, "GIF option");
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }
}

using System.Text.RegularExpressions;

namespace CMG.Browser.Scripting.Recording;

internal sealed record ScriptAutoCaption(string Message, string? TargetSelector)
{
    private static readonly Regex Placeholder = new("\\{([a-zA-Z]+)\\}", RegexOptions.Compiled);

    public static bool TryCreate(BrowserScriptAction action, out ScriptAutoCaption caption)
    {
        caption = new(string.Empty, null);
        if (!Enabled(action) || !IsNarratable(action.Name))
        {
            return false;
        }

        var selector = SelectorFor(action);
        var target = action.Arguments.Count > 1 ? action.Arguments[1] : string.Empty;
        var template = action.Options.GetValueOrDefault("captionTemplate") ?? DefaultTemplate(action.Name);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["action"] = action.Name,
            ["selector"] = selector ?? action.Arguments.FirstOrDefault() ?? string.Empty,
            ["target"] = target,
            ["line"] = action.LineNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["arguments"] = string.Join(' ', action.Arguments)
        };
        var message = Placeholder.Replace(template, match => values.TryGetValue(match.Groups[1].Value, out var value)
            ? value
            : throw new ScriptExecutionException($"{action.Name} option captionTemplate= contains unknown placeholder '{match.Value}'."));
        caption = new(message.Trim(), selector);
        return true;
    }

    private static bool Enabled(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("autoCaptions", out var value))
        {
            return false;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{action.Name} option autoCaptions= must be true or false.")
        };
    }

    private static string DefaultTemplate(string name) => name.ToLowerInvariant() switch
    {
        "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" => "Click {selector}",
        "fill" or "type" or "presssequentially" or "clear" => "Enter text in {selector}",
        "hover" => "Hover over {selector}",
        "draganddrop" or "dragto" => "Drag {selector} to {target}",
        "navigate" or "goto" or "visit" => "Navigate to {selector}",
        _ => "Verify {selector}"
    };

    private static bool IsNarratable(string name)
    {
        name = name.ToLowerInvariant();
        return name is "click" or "dblclick" or "doubleclick" or "rightclick" or "contextclick" or "tap" or "touchtap" or
            "fill" or "type" or "presssequentially" or "clear" or "hover" or "select" or "selectoption" or "check" or "uncheck" or
            "focus" or "blur" or "highlight" or "draganddrop" or "dragto" or "navigate" or "goto" or "visit" or
            "asserttext" or "expecttext" or "tohavetext" or "expectvisible" or "tobevisible" or "expecthidden" or "tobehidden" or
            "expectvalue" or "tohavevalue" or "expectenabled" or "tobeenabled" or "expectdisabled" or "tobedisabled";
    }

    private static string? SelectorFor(BrowserScriptAction action)
    {
        var name = action.Name.ToLowerInvariant();
        return action.Arguments.Count > 0 && name is not ("navigate" or "goto" or "visit")
            ? action.Arguments[0]
            : null;
    }
}

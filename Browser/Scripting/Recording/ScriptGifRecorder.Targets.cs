
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private ElementPoint ResolveMoveMouseTarget(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return pointer.Position;
        }

        if (action.Options.ContainsKey("selector") || action.Options.ContainsKey("edge"))
        {
            return ResolveElementEdgeTarget(action);
        }

        var hasAlias = action.Arguments.Count is 1;
        var hasCoordinates = action.Options.ContainsKey("x") || action.Options.ContainsKey("y");

        if (hasAlias == hasCoordinates)
        {
            throw new ScriptExecutionException("moveMouse requires either one alias argument or x=<pixels> y=<pixels> options.");
        }

        var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl);
        var target = hasAlias
            ? ResolveAlias(action.Arguments[0], viewport)
            : new ElementPoint(ParseCoordinate(action, "x"), ParseCoordinate(action, "y"));

        if (target.X < 0 || target.Y < 0 || target.X > viewport.Width || target.Y > viewport.Height)
        {
            throw new ScriptExecutionException(
                $"moveMouse target ({FormatNumber(target.X)}, {FormatNumber(target.Y)}) is outside the current viewport {FormatNumber(viewport.Width)}x{FormatNumber(viewport.Height)}.");
        }

        return target;
    }

    private ElementPoint ResolveElementEdgeTarget(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return pointer.Position;
        }

        var selector = action.Options.TryGetValue("selector", out var selectorOption)
            ? selectorOption
            : action.Arguments.Count is 1
                ? action.Arguments[0]
                : null;

        if (string.IsNullOrWhiteSpace(selector))
        {
            throw new ScriptExecutionException("moveMouse element-edge targeting requires selector=<selector> or one selector argument.");
        }

        if (!action.Options.TryGetValue("edge", out var edge) || string.IsNullOrWhiteSpace(edge))
        {
            throw new ScriptExecutionException("moveMouse element-edge targeting requires edge=<top|bottom|left|right|center|topLeft|topRight|bottomLeft|bottomRight>.");
        }

        if (action.Options.ContainsKey("x") || action.Options.ContainsKey("y"))
        {
            throw new ScriptExecutionException("moveMouse cannot combine selector/edge targeting with x/y coordinates.");
        }

        if (action.Options.ContainsKey("selector") && action.Arguments.Count > 0)
        {
            throw new ScriptExecutionException("moveMouse selector targeting accepts either selector=<selector> or one selector argument, not both.");
        }

        var inset = action.Options.TryGetValue("inset", out var insetValue)
            ? ParseNonNegativeNumber(insetValue, "inset")
            : 16;

        var viewport = devToolsClient.GetViewportSize(remoteDebuggingUrl);
        var box = devToolsClient.GetElementBox(remoteDebuggingUrl, selector);
        var left = Clamp(box.X + inset, 0, viewport.Width);
        var right = Clamp(box.X + box.Width - inset, 0, viewport.Width);
        var top = Clamp(box.Y + inset, 0, viewport.Height);
        var bottom = Clamp(box.Y + box.Height - inset, 0, viewport.Height);
        var centerX = Clamp(box.X + box.Width / 2, 0, viewport.Width);
        var centerY = Clamp(box.Y + box.Height / 2, 0, viewport.Height);

        return edge.ToLowerInvariant() switch
        {
            "center" => new ElementPoint(centerX, centerY),
            "top" => new ElementPoint(centerX, top),
            "bottom" => new ElementPoint(centerX, bottom),
            "left" => new ElementPoint(left, centerY),
            "right" => new ElementPoint(right, centerY),
            "topleft" => new ElementPoint(left, top),
            "topright" => new ElementPoint(right, top),
            "bottomleft" => new ElementPoint(left, bottom),
            "bottomright" => new ElementPoint(right, bottom),
            _ => throw new ScriptExecutionException("moveMouse edge must be one of: top, bottom, left, right, center, topLeft, topRight, bottomLeft, bottomRight.")
        };
    }

    private static ElementPoint ResolveAlias(string alias, ViewportSize viewport)
    {
        var inset = Math.Min(16, Math.Max(0, Math.Min(viewport.Width, viewport.Height) / 2));
        var centerX = viewport.Width / 2;
        var centerY = viewport.Height / 2;
        var right = Math.Max(0, viewport.Width - inset);
        var bottom = Math.Max(0, viewport.Height - inset);

        return alias.ToLowerInvariant() switch
        {
            "center" => new ElementPoint(centerX, centerY),
            "top" => new ElementPoint(centerX, inset),
            "bottom" => new ElementPoint(centerX, bottom),
            "left" => new ElementPoint(inset, centerY),
            "right" => new ElementPoint(right, centerY),
            "topleft" => new ElementPoint(inset, inset),
            "topright" => new ElementPoint(right, inset),
            "bottomleft" => new ElementPoint(inset, bottom),
            "bottomright" => new ElementPoint(right, bottom),
            _ => throw new ScriptExecutionException($"Unknown moveMouse alias '{alias}'. Supported aliases: {string.Join(", ", MouseAliases)}.")
        };
    }

    private static double ParseCoordinate(BrowserScriptAction action, string name)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            throw new ScriptExecutionException($"Missing required option '{name}'.");
        }

        var number = ParseNonNegativeNumber(value, name);
        return number;
    }

    private static double ParseNonNegativeNumber(string value, string name)
    {
        if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) || number < 0)
        {
            throw new ScriptExecutionException($"moveMouse option '{name}' must be a non-negative number.");
        }

        return number;
    }

    private static double Clamp(double value, double min, double max) =>
        Math.Min(max, Math.Max(min, value));

    private static string FormatNumber(double value) =>
        value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
}

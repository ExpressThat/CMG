namespace CMG.Browser.Scripting;

public static class BrowserMouseTargetResolver
{
    private static readonly string[] Aliases = ["center", "top", "bottom", "left", "right", "topLeft", "topRight", "bottomLeft", "bottomRight"];

    public static ElementPoint Resolve(string remoteDebuggingUrl, IBrowserAutomationClient client, BrowserScriptAction action)
    {
        if (action.Options.ContainsKey("selector") || action.Options.ContainsKey("edge"))
        {
            return ResolveElementEdge(remoteDebuggingUrl, client, action);
        }

        var hasAlias = action.Arguments.Count is 1;
        var hasCoordinates = action.Options.ContainsKey("x") || action.Options.ContainsKey("y");
        if (hasAlias == hasCoordinates)
        {
            throw new ScriptExecutionException($"{action.Name} requires either one alias argument or x=<pixels> y=<pixels> options.");
        }

        var viewport = client.GetViewportSize(remoteDebuggingUrl);
        var target = hasAlias
            ? ResolveAlias(action.Arguments[0], viewport)
            : new ElementPoint(ParseCoordinate(action, "x"), ParseCoordinate(action, "y"));
        EnsureInViewport(action.Name, target, viewport);
        return target;
    }

    private static ElementPoint ResolveElementEdge(string remoteDebuggingUrl, IBrowserAutomationClient client, BrowserScriptAction action)
    {
        var selector = action.Options.TryGetValue("selector", out var selectorOption)
            ? selectorOption
            : action.Arguments.Count is 1 ? action.Arguments[0] : null;
        if (string.IsNullOrWhiteSpace(selector))
        {
            throw new ScriptExecutionException($"{action.Name} selector targeting requires selector=<selector> or one selector argument.");
        }

        if (!action.Options.TryGetValue("edge", out var edge) || string.IsNullOrWhiteSpace(edge))
        {
            throw new ScriptExecutionException($"{action.Name} selector targeting requires edge=<top|bottom|left|right|center|topLeft|topRight|bottomLeft|bottomRight>.");
        }

        if (action.Options.ContainsKey("x") || action.Options.ContainsKey("y") || action.Options.ContainsKey("selector") && action.Arguments.Count > 0)
        {
            throw new ScriptExecutionException($"{action.Name} selector targeting cannot be combined with x/y coordinates or duplicate selector arguments.");
        }

        var inset = action.Options.TryGetValue("inset", out var value) ? ParseNonNegativeNumber(value, "inset") : 16;
        var viewport = client.GetViewportSize(remoteDebuggingUrl);
        var box = client.GetElementBox(remoteDebuggingUrl, selector);
        return EdgePoint(action.Name, edge, box, viewport, inset);
    }

    private static ElementPoint EdgePoint(string actionName, string edge, ElementBox box, ViewportSize viewport, double inset)
    {
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
            _ => throw new ScriptExecutionException($"{actionName} edge must be one of: {string.Join(", ", Aliases)}.")
        };
    }

    private static ElementPoint ResolveAlias(string alias, ViewportSize viewport)
    {
        var inset = Math.Min(16, Math.Max(0, Math.Min(viewport.Width, viewport.Height) / 2));
        return alias.ToLowerInvariant() switch
        {
            "center" => new ElementPoint(viewport.Width / 2, viewport.Height / 2),
            "top" => new ElementPoint(viewport.Width / 2, inset),
            "bottom" => new ElementPoint(viewport.Width / 2, Math.Max(0, viewport.Height - inset)),
            "left" => new ElementPoint(inset, viewport.Height / 2),
            "right" => new ElementPoint(Math.Max(0, viewport.Width - inset), viewport.Height / 2),
            "topleft" => new ElementPoint(inset, inset),
            "topright" => new ElementPoint(Math.Max(0, viewport.Width - inset), inset),
            "bottomleft" => new ElementPoint(inset, Math.Max(0, viewport.Height - inset)),
            "bottomright" => new ElementPoint(Math.Max(0, viewport.Width - inset), Math.Max(0, viewport.Height - inset)),
            _ => throw new ScriptExecutionException($"Unknown mouse alias '{alias}'. Supported aliases: {string.Join(", ", Aliases)}.")
        };
    }

    private static void EnsureInViewport(string actionName, ElementPoint point, ViewportSize viewport)
    {
        if (point.X < 0 || point.Y < 0 || point.X > viewport.Width || point.Y > viewport.Height)
        {
            throw new ScriptExecutionException($"{actionName} target ({Format(point.X)}, {Format(point.Y)}) is outside the current viewport {Format(viewport.Width)}x{Format(viewport.Height)}.");
        }
    }

    private static double ParseCoordinate(BrowserScriptAction action, string name) =>
        action.Options.TryGetValue(name, out var value) ? ParseNonNegativeNumber(value, name) : throw new ScriptExecutionException($"Missing required option '{name}'.");

    private static double ParseNonNegativeNumber(string value, string name) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) && number >= 0
            ? number
            : throw new ScriptExecutionException($"Mouse option '{name}' must be a non-negative number.");

    private static double Clamp(double value, double min, double max) => Math.Min(max, Math.Max(min, value));

    private static string Format(double value) => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
}

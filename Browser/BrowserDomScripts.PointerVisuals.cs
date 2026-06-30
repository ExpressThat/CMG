namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    private static string ModeFor(PointerVisualOptions? visual)
    {
        var value = visual ?? PointerVisualOptions.Default;
        return $"{value.Theme}:{value.Color}:{value.SizePixels}:{value.Shadow}";
    }

    private static string CursorSvg(PointerVisualOptions? visual)
    {
        var value = visual ?? PointerVisualOptions.Default;
        var color = value.Color ?? "#2563eb";
        return value.Theme switch
        {
            PointerTheme.Hand => HandCursorSvg(color),
            PointerTheme.Dot => DotCursorSvg(color),
            PointerTheme.Ring => RingCursorSvg(color),
            PointerTheme.Branded => BrandedCursorSvg(color),
            PointerTheme.Touch => TouchCursorSvg(color),
            _ => ArrowCursorSvg(value.Color)
        };
    }

    private static string CursorTransform(PointerVisualOptions? visual, bool pressed)
    {
        var value = visual ?? PointerVisualOptions.Default;
        var scale = PointerScale(value, pressed);
        return IsCentered(value.Theme)
            ? $"translate(-50%, -50%) scale({Invariant(scale)})"
            : $"translate(-3px, -3px) scale({Invariant(scale)})";
    }

    private static string CursorFilter(PointerVisualOptions? visual, bool pressed)
    {
        var shadow = (visual ?? PointerVisualOptions.Default).Shadow;
        if (shadow is PointerShadow.None)
        {
            return pressed ? "saturate(1.25)" : "none";
        }

        var filter = shadow switch
        {
            PointerShadow.Light => "drop-shadow(0 1px 2px rgba(0,0,0,.28))",
            PointerShadow.Strong => "drop-shadow(0 3px 7px rgba(0,0,0,.62))",
            _ => pressed
                ? "drop-shadow(0 1px 2px rgba(0,0,0,.48))"
                : "drop-shadow(0 2px 3px rgba(0,0,0,.4))"
        };
        return pressed ? $"{filter} saturate(1.25)" : filter;
    }

    private static double PointerScale(PointerVisualOptions visual, bool pressed)
    {
        var baseSize = IsCentered(visual.Theme) ? 34d : 26d;
        var scale = visual.SizePixels is int size ? size / baseSize : 1d;
        return pressed && !IsCentered(visual.Theme) ? scale * .88d : scale;
    }

    private static bool IsCentered(PointerTheme theme) =>
        theme is PointerTheme.Dot or PointerTheme.Ring or PointerTheme.Touch;

    private static string ArrowCursorSvg(string? color)
    {
        var fill = color ?? "#fff";
        var stroke = color is null ? "#111" : "#0f172a";
        return $"<svg width=\"26\" height=\"34\" viewBox=\"0 0 26 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><path d=\"M3 2L22 19L13.2 20.1L9.2 31L3 2Z\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"2\" stroke-linejoin=\"round\"/><path d=\"M13.2 20.1L18.6 29.4\" stroke=\"{stroke}\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>";
    }

    private static string HandCursorSvg(string color) =>
        $"<svg width=\"30\" height=\"34\" viewBox=\"0 0 30 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><path d=\"M9 31c-2.7-4.4-4-7.4-4-9.1v-5.3c0-1.2.9-2.1 2.1-2.1 1 0 1.8.7 2 1.6V7.2c0-1.2.9-2.2 2.1-2.2s2.2 1 2.2 2.2v7.2-9.1c0-1.2 1-2.2 2.2-2.2s2.1 1 2.1 2.2v9.3-7.1c0-1.2 1-2.2 2.2-2.2s2.1 1 2.1 2.2v9.4-5.2c0-1.2.9-2.1 2.1-2.1s2.2.9 2.2 2.1v8.9c0 3.9-1.2 7.3-3.7 10.4H9Z\" fill=\"{color}\" stroke=\"#0f172a\" stroke-width=\"2\" stroke-linejoin=\"round\"/></svg>";

    private static string DotCursorSvg(string color) =>
        $"<svg width=\"34\" height=\"34\" viewBox=\"0 0 34 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><circle cx=\"17\" cy=\"17\" r=\"7\" fill=\"{color}\" stroke=\"#fff\" stroke-width=\"3\"/></svg>";

    private static string RingCursorSvg(string color) =>
        $"<svg width=\"34\" height=\"34\" viewBox=\"0 0 34 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><circle cx=\"17\" cy=\"17\" r=\"11\" fill=\"none\" stroke=\"{color}\" stroke-width=\"4\"/><circle cx=\"17\" cy=\"17\" r=\"2.5\" fill=\"{color}\"/></svg>";

    private static string BrandedCursorSvg(string color) =>
        $"<svg width=\"30\" height=\"34\" viewBox=\"0 0 30 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><path d=\"M4 2L26 17L16 19L12 31L4 2Z\" fill=\"{color}\" stroke=\"#0f172a\" stroke-width=\"2\" stroke-linejoin=\"round\"/><circle cx=\"14\" cy=\"15\" r=\"4\" fill=\"#fff\" opacity=\".88\"/></svg>";

    private static string TouchCursorSvg(string color) =>
        $"<svg width=\"34\" height=\"34\" viewBox=\"0 0 34 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><circle cx=\"17\" cy=\"17\" r=\"11\" fill=\"rgba(37,99,235,.18)\" stroke=\"{color}\" stroke-width=\"3\"/><circle cx=\"17\" cy=\"17\" r=\"4\" fill=\"{color}\"/></svg>";
}

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    private static string CaptionMode(BrowserCaptionOptions? options)
    {
        var value = options ?? BrowserCaptionOptions.Default;
        return $"{value.Style}:{value.Position}:{value.Severity}:{value.Size}";
    }

    private static string CaptionStyleText(BrowserCaptionOptions? options)
    {
        var value = options ?? BrowserCaptionOptions.Default;
        var position = value.Position is CaptionPosition.Auto ? CaptionPosition.Top : value.Position;
        return "all:initial;position:fixed;z-index:2147483646;margin:0;border:0;width:max-content;"
            + "box-sizing:border-box;pointer-events:none;white-space:pre-wrap;overflow-wrap:anywhere;"
            + CaptionPositionCss(position)
            + CaptionPresetCss(value.Style, value.Severity)
            + CaptionSizeCss(value.Style, value.Size);
    }

    private static string CaptionPositionCss(CaptionPosition position) =>
        position switch
        {
            CaptionPosition.Bottom => "left:50%;bottom:16px;top:auto;right:auto;transform:translateX(-50%);",
            CaptionPosition.Left => "left:16px;top:50%;right:auto;bottom:auto;transform:translateY(-50%);",
            CaptionPosition.Right => "right:16px;top:50%;left:auto;bottom:auto;transform:translateY(-50%);",
            _ => "left:50%;top:12px;right:auto;bottom:auto;transform:translateX(-50%);"
        };

    private static string CaptionPresetCss(CaptionStyle style, CaptionSeverity severity)
    {
        var (background, color, accent) = CaptionColors(severity);
        var common = $"background:{background};color:{color};box-shadow:0 10px 28px rgba(0,0,0,.24);";
        return style switch
        {
            CaptionStyle.Teaching => common + $"max-width:min(860px,calc(100vw - 32px));padding:14px 20px 14px 18px;border-left:6px solid {accent};border-radius:6px;font:600 16px/1.45 system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;text-align:left;",
            CaptionStyle.Qa => common + $"max-width:min(780px,calc(100vw - 32px));padding:11px 16px;border:2px solid {accent};border-radius:4px;font:700 13px/1.4 ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;text-align:center;text-transform:uppercase;",
            CaptionStyle.BugReport => common + $"max-width:min(820px,calc(100vw - 32px));padding:12px 18px;border-top:5px solid {accent};border-radius:4px;font:700 14px/1.45 system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;text-align:left;",
            CaptionStyle.Compact => common + "max-width:min(520px,calc(100vw - 32px));min-height:0;padding:6px 10px;border-radius:4px;font:600 12px/1.3 system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;text-align:center;",
            _ => common + "max-width:min(760px,calc(100vw - 32px));min-height:42px;padding:10px 16px;border-radius:8px;font:600 14px/1.45 system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;text-align:center;"
        };
    }

    private static (string Background, string Color, string Accent) CaptionColors(CaptionSeverity severity) =>
        severity switch
        {
            CaptionSeverity.Success => ("#064e3b", "#ecfdf5", "#34d399"),
            CaptionSeverity.Warning => ("#78350f", "#fffbeb", "#f59e0b"),
            CaptionSeverity.Error => ("#7f1d1d", "#fef2f2", "#f87171"),
            _ => ("#111827", "#ffffff", "#60a5fa")
        };

    private static string CaptionSizeCss(CaptionStyle style, CaptionSize size)
    {
        if (size is CaptionSize.Normal) return string.Empty;
        var extra = size is CaptionSize.ExtraLarge;
        var pixels = style switch
        {
            CaptionStyle.Compact => extra ? 20 : 16,
            CaptionStyle.Qa => extra ? 21 : 17,
            _ => extra ? 24 : 19
        };
        return $"font-size:{pixels}px;line-height:1.45;";
    }
}

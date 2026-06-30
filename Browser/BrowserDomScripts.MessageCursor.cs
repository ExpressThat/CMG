using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowMessageBar(string message) =>
        $$"""
        (() => {
          const promote = element => {
            if (typeof element.showPopover === 'function') {
              if (element.matches(':popover-open')) element.hidePopover();
              element.showPopover();
            }
          };
          let bar = document.getElementById('__cmg_message_bar');
          if (!bar) {
            bar = document.createElement('div');
            bar.id = '__cmg_message_bar';
            bar.setAttribute('popover', 'manual');
            bar.setAttribute('role', 'status');
            bar.setAttribute('aria-live', 'polite');
            bar.style.all = 'initial';
            bar.style.position = 'fixed';
            bar.style.left = '50%';
            bar.style.top = '12px';
            bar.style.zIndex = '2147483646';
            bar.style.width = 'max-content';
            bar.style.maxWidth = 'min(760px, calc(100vw - 32px))';
            bar.style.minHeight = '42px';
            bar.style.margin = '0';
            bar.style.padding = '10px 16px';
            bar.style.border = '0';
            bar.style.background = '#111827';
            bar.style.color = '#ffffff';
            bar.style.font = '600 14px/1.45 system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
            bar.style.textAlign = 'center';
            bar.style.boxShadow = '0 10px 28px rgba(0,0,0,.24)';
            bar.style.boxSizing = 'border-box';
            bar.style.pointerEvents = 'none';
            bar.style.borderRadius = '8px';
            bar.style.transform = 'translateX(-50%)';
            bar.style.whiteSpace = 'pre-wrap';
            bar.style.overflowWrap = 'anywhere';
            const text = document.createElement('div');
            text.id = '__cmg_message_bar_text';
            bar.appendChild(text);
            document.documentElement.appendChild(bar);
          }
          document.getElementById('__cmg_message_bar_text').textContent = {{JsonString(message)}};
          promote(bar);
          return true;
        })()
        """;

    public static string PromoteMessageBar() =>
        "(() => { const bar = document.getElementById('__cmg_message_bar'); if (!bar || typeof bar.showPopover !== 'function') return true; if (bar.matches(':popover-open')) bar.hidePopover(); bar.showPopover(); return true; })()";

    public static string MoveDomCursor(
        ElementPoint point,
        ClickPulseStyle? pulseStyle = null,
        bool pressed = false,
        bool trail = false,
        bool breadcrumb = false,
        bool touch = false) =>
        $$"""
        (() => {
          const promote = element => {
            if (typeof element.showPopover === 'function') {
              if (element.matches(':popover-open')) element.hidePopover();
              element.showPopover();
            }
          };
          let cursor = document.getElementById('__cmg_virtual_cursor');
          if (!cursor) {
            cursor = document.createElement('div');
            cursor.id = '__cmg_virtual_cursor';
            cursor.setAttribute('popover', 'manual');
            cursor.style.all = 'initial';
            cursor.style.position = 'fixed';
            cursor.style.left = '0px';
            cursor.style.top = '0px';
            cursor.style.width = '26px';
            cursor.style.height = '34px';
            cursor.style.margin = '0';
            cursor.style.padding = '0';
            cursor.style.border = '0';
            cursor.style.background = 'transparent';
            cursor.style.overflow = 'visible';
            cursor.style.color = 'transparent';
            cursor.style.zIndex = '2147483647';
            cursor.style.pointerEvents = 'none';
            cursor.style.transform = 'translate(-3px, -3px)';
            cursor.style.filter = 'drop-shadow(0 2px 3px rgba(0,0,0,.4))';
            cursor.innerHTML = '<svg width="26" height="34" viewBox="0 0 26 34" xmlns="http://www.w3.org/2000/svg" aria-hidden="true"><path d="M3 2L22 19L13.2 20.1L9.2 31L3 2Z" fill="#fff" stroke="#111" stroke-width="2" stroke-linejoin="round"/><path d="M13.2 20.1L18.6 29.4" stroke="#111" stroke-width="2.5" stroke-linecap="round"/></svg>';
            document.documentElement.appendChild(cursor);
          }
          if (cursor.dataset.cmgPointerMode !== '{{(touch ? "touch" : "arrow")}}') {
            cursor.dataset.cmgPointerMode = '{{(touch ? "touch" : "arrow")}}';
            cursor.innerHTML = '{{(touch ? TouchCursorSvg() : ArrowCursorSvg())}}';
          }
          cursor.style.left = '{{Invariant(point.X)}}px';
          cursor.style.top = '{{Invariant(point.Y)}}px';
          cursor.style.transform = '{{(touch ? "translate(-50%, -50%)" : pressed ? "translate(-3px, -3px) scale(.88)" : "translate(-3px, -3px)")}}';
          cursor.style.filter = '{{(pressed ? "drop-shadow(0 1px 2px rgba(0,0,0,.48)) saturate(1.25)" : "drop-shadow(0 2px 3px rgba(0,0,0,.4))")}}';
          {{PulseScript(point, pulseStyle)}}
          {{TrailScript(point, trail)}}
          {{BreadcrumbScript(point, breadcrumb)}}
          promote(cursor);
          return true;
        })()
        """;

    public static string RemoveDomCursor() =>
        "(() => { const cursor = document.getElementById('__cmg_virtual_cursor'); if (cursor?.matches?.(':popover-open')) cursor.hidePopover(); cursor?.remove(); document.getElementById('__cmg_cursor_trail')?.remove(); document.querySelectorAll('[data-cmg-cursor-breadcrumb]').forEach(node => node.remove()); return true; })()";

    public static string RemoveDefaultDragGhost() =>
        "(() => { const ghost = document.getElementById('__cmg_default_drag_ghost'); if (ghost?.matches?.(':popover-open')) ghost.hidePopover(); ghost?.remove(); return true; })()";

    public static string ClearPointerEvidence() =>
        "(() => { document.getElementById('__cmg_cursor_trail')?.remove(); document.querySelectorAll('[data-cmg-cursor-breadcrumb]').forEach(node => node.remove()); return true; })()";

    private static string ArrowCursorSvg() =>
        "<svg width=\"26\" height=\"34\" viewBox=\"0 0 26 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><path d=\"M3 2L22 19L13.2 20.1L9.2 31L3 2Z\" fill=\"#fff\" stroke=\"#111\" stroke-width=\"2\" stroke-linejoin=\"round\"/><path d=\"M13.2 20.1L18.6 29.4\" stroke=\"#111\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>";

    private static string TouchCursorSvg() =>
        "<svg width=\"34\" height=\"34\" viewBox=\"0 0 34 34\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><circle cx=\"17\" cy=\"17\" r=\"11\" fill=\"rgba(37,99,235,.18)\" stroke=\"#2563eb\" stroke-width=\"3\"/><circle cx=\"17\" cy=\"17\" r=\"4\" fill=\"#2563eb\"/></svg>";

    private static string PulseScript(ElementPoint point, ClickPulseStyle? pulseStyle)
    {
        if (pulseStyle is null)
        {
            return string.Empty;
        }

        return pulseStyle is ClickPulseStyle.None
            ? "document.getElementById('__cmg_cursor_pulse')?.remove();"
            : $$"""
              let pulse = document.getElementById('__cmg_cursor_pulse');
              if (!pulse) {
                pulse = document.createElement('div');
                pulse.id = '__cmg_cursor_pulse';
                pulse.setAttribute('popover', 'manual');
                pulse.style.all = 'initial';
                pulse.style.position = 'fixed';
                pulse.style.margin = '0';
                pulse.style.padding = '0';
                pulse.style.border = '0';
                pulse.style.background = 'transparent';
                pulse.style.pointerEvents = 'none';
                pulse.style.zIndex = '2147483646';
                pulse.style.overflow = 'visible';
                document.documentElement.appendChild(pulse);
              }
              pulse.style.left = '{{Invariant(point.X)}}px';
              pulse.style.top = '{{Invariant(point.Y)}}px';
              {{PulseStyleScript(pulseStyle.Value)}}
              promote(pulse);
              """;
    }

    private static string PulseStyleScript(ClickPulseStyle style) =>
        style switch
        {
            ClickPulseStyle.Dot =>
                "pulse.style.width='14px';pulse.style.height='14px';pulse.style.transform='translate(-50%,-50%)';pulse.style.borderRadius='999px';pulse.style.background='#2563eb';pulse.style.boxShadow='0 0 0 4px rgba(37,99,235,.18)';pulse.innerHTML='';",
            ClickPulseStyle.Crosshair =>
                "pulse.style.width='38px';pulse.style.height='38px';pulse.style.transform='translate(-50%,-50%)';pulse.style.borderRadius='999px';pulse.style.background='transparent';pulse.style.boxShadow='none';pulse.innerHTML='<svg width=\"38\" height=\"38\" viewBox=\"0 0 38 38\" xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"true\"><circle cx=\"19\" cy=\"19\" r=\"12\" fill=\"none\" stroke=\"#dc2626\" stroke-width=\"2.5\"/><path d=\"M19 2v10M19 26v10M2 19h10M26 19h10\" stroke=\"#dc2626\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>';",
            ClickPulseStyle.Ripple =>
                "pulse.style.width='46px';pulse.style.height='46px';pulse.style.transform='translate(-50%,-50%)';pulse.style.borderRadius='999px';pulse.style.background='rgba(37,99,235,.10)';pulse.style.boxShadow='0 0 0 2px rgba(37,99,235,.85),0 0 0 10px rgba(37,99,235,.18)';pulse.innerHTML='';",
            _ =>
                "pulse.style.width='34px';pulse.style.height='34px';pulse.style.transform='translate(-50%,-50%)';pulse.style.borderRadius='999px';pulse.style.background='transparent';pulse.style.boxShadow='0 0 0 3px #2563eb,0 0 0 7px rgba(37,99,235,.18)';pulse.innerHTML='';"
        };

    private static string TrailScript(ElementPoint point, bool enabled) =>
        enabled
            ? $$"""
              let trail = document.getElementById('__cmg_cursor_trail');
              if (!trail) {
                trail = document.createElement('div');
                trail.id = '__cmg_cursor_trail';
                trail.setAttribute('popover', 'manual');
                trail.style.cssText = 'all:initial;position:fixed;left:0;top:0;width:100vw;height:100vh;pointer-events:none;z-index:2147483645;overflow:visible;';
                trail.innerHTML = '<svg width="100%" height="100%" style="position:fixed;left:0;top:0;overflow:visible" aria-hidden="true"><polyline id="__cmg_cursor_trail_line" fill="none" stroke="#2563eb" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" opacity=".48"/></svg>';
                document.documentElement.appendChild(trail);
              }
              const line = document.getElementById('__cmg_cursor_trail_line');
              const next = '{{Invariant(point.X)}},{{Invariant(point.Y)}}';
              const points = (line.getAttribute('points') || '').trim().split(/\s+/).filter(Boolean);
              points.push(next);
              line.setAttribute('points', points.slice(-42).join(' '));
              promote(trail);
              """
            : "document.getElementById('__cmg_cursor_trail')?.remove();";

    private static string BreadcrumbScript(ElementPoint point, bool enabled) =>
        enabled
            ? $$"""
              const crumb = document.createElement('div');
              crumb.setAttribute('data-cmg-cursor-breadcrumb', 'true');
              crumb.setAttribute('popover', 'manual');
              crumb.style.cssText = 'all:initial;position:fixed;left:{{Invariant(point.X)}}px;top:{{Invariant(point.Y)}}px;width:7px;height:7px;transform:translate(-50%,-50%);border-radius:999px;background:#2563eb;box-shadow:0 0 0 2px rgba(255,255,255,.85);pointer-events:none;z-index:2147483645;';
              document.documentElement.appendChild(crumb);
              promote(crumb);
              const crumbs = Array.from(document.querySelectorAll('[data-cmg-cursor-breadcrumb]'));
              crumbs.slice(0, Math.max(0, crumbs.length - 28)).forEach(node => node.remove());
              """
            : "document.querySelectorAll('[data-cmg-cursor-breadcrumb]').forEach(node => node.remove());";
}

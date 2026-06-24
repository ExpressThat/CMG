using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static class BrowserDomScripts
{
    public static string ElementAction(string selector, string body) =>
        $$"""
        (() => {
          const element = document.querySelector({{JsonString(selector)}});
          if (!element) return false;
          {{body}}
        })()
        """;

    public static string ScrollIntoView(string selector) =>
        $"(() => {{ const element = document.querySelector({JsonString(selector)}); if (!element) return false; element.scrollIntoView({{ block: 'center', inline: 'center' }}); return true; }})()";

    public static string ElementRect(string selector) =>
        $$"""
        (() => {
          const element = document.querySelector({{JsonString(selector)}});
          if (!element) return null;
          const rect = element.getBoundingClientRect();
          return JSON.stringify({
            x: rect.left,
            y: rect.top,
            width: rect.width,
            height: rect.height
          });
        })()
        """;

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

    public static string MoveDomCursor(ElementPoint point) =>
        $$"""
        (() => {
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
          cursor.style.left = '{{Invariant(point.X)}}px';
          cursor.style.top = '{{Invariant(point.Y)}}px';
          if (typeof cursor.showPopover === 'function') {
            if (cursor.matches(':popover-open')) cursor.hidePopover();
            cursor.showPopover();
          }
          return true;
        })()
        """;

    public static string RemoveDomCursor() =>
        "(() => { const cursor = document.getElementById('__cmg_virtual_cursor'); if (cursor?.matches?.(':popover-open')) cursor.hidePopover(); cursor?.remove(); return true; })()";

    public static string RemoveDefaultDragGhost() =>
        "(() => { const ghost = document.getElementById('__cmg_default_drag_ghost'); if (ghost?.matches?.(':popover-open')) ghost.hidePopover(); ghost?.remove(); return true; })()";

    public static string MoveMouse(ElementPoint point, int buttons) =>
        MouseEventScript(point, buttons, "move");

    public static string MouseDown(ElementPoint point) =>
        MouseEventScript(point, 1, "down");

    public static string MouseUp(ElementPoint point) =>
        MouseEventScript(point, 0, "up");

    public static string DragAndDrop(string sourceSelector, string targetSelector) =>
        $$"""
        (() => {
          const source = document.querySelector({{JsonString(sourceSelector)}});
          const target = document.querySelector({{JsonString(targetSelector)}});
          if (!source || !target) return false;
          const dataTransfer = new DataTransfer();
          const sourceRect = source.getBoundingClientRect();
          const targetRect = target.getBoundingClientRect();
          const start = { clientX: sourceRect.left + sourceRect.width / 2, clientY: sourceRect.top + sourceRect.height / 2, bubbles: true, cancelable: true, dataTransfer };
          const end = { clientX: targetRect.left + targetRect.width / 2, clientY: targetRect.top + targetRect.height / 2, bubbles: true, cancelable: true, dataTransfer };
          source.dispatchEvent(new DragEvent('dragstart', start));
          source.dispatchEvent(new DragEvent('drag', end));
          target.dispatchEvent(new DragEvent('dragenter', end));
          target.dispatchEvent(new DragEvent('dragover', end));
          target.dispatchEvent(new DragEvent('drop', end));
          source.dispatchEvent(new DragEvent('dragend', end));
          return true;
        })()
        """;

    public static string BeginDrag(string sourceSelector, ElementPoint point) =>
        $$"""
        (() => {
          const source = document.querySelector({{JsonString(sourceSelector)}});
          if (!source) return false;
          const dataTransfer = new DataTransfer();
          let customDragImageSet = false;
          const originalSetDragImage = dataTransfer.setDragImage?.bind(dataTransfer);
          if (originalSetDragImage) {
            try { Object.defineProperty(dataTransfer, 'setDragImage', { configurable: true, value: () => { customDragImageSet = true; } }); } catch {}
          }
          window.__cmgRecordingDrag = { source, dataTransfer, defaultGhost: null };
          const eventOptions = { bubbles: true, cancelable: true, clientX: {{Invariant(point.X)}}, clientY: {{Invariant(point.Y)}}, dataTransfer };
          source.dispatchEvent(new DragEvent('dragstart', eventOptions));
          if (!customDragImageSet) {
            const existing = document.getElementById('__cmg_default_drag_ghost');
            existing?.remove();
            const sourceRect = source.getBoundingClientRect();
            const ghost = document.createElement('div');
            ghost.id = '__cmg_default_drag_ghost';
            ghost.setAttribute('popover', 'manual');
            ghost.style.all = 'initial';
            ghost.style.position = 'fixed';
            ghost.style.left = `${eventOptions.clientX}px`;
            ghost.style.top = `${eventOptions.clientY}px`;
            ghost.style.zIndex = '2147483645';
            ghost.style.width = `${Math.max(1, sourceRect.width)}px`;
            ghost.style.height = `${Math.max(1, sourceRect.height)}px`;
            ghost.style.margin = '0';
            ghost.style.padding = '0';
            ghost.style.border = '0';
            ghost.style.background = 'transparent';
            ghost.style.overflow = 'visible';
            ghost.style.pointerEvents = 'none';
            ghost.style.opacity = '.72';
            ghost.style.transform = 'translate(-50%, -50%) rotate(-1deg)';
            ghost.style.filter = 'drop-shadow(0 12px 18px rgba(0,0,0,.24))';
            const clone = source.cloneNode(true);
            clone.removeAttribute?.('id');
            clone.removeAttribute?.('popover');
            clone.style.pointerEvents = 'none';
            clone.style.boxSizing = 'border-box';
            clone.style.width = `${Math.max(1, sourceRect.width)}px`;
            clone.style.height = `${Math.max(1, sourceRect.height)}px`;
            clone.style.maxWidth = 'none';
            clone.style.maxHeight = 'none';
            clone.style.margin = '0';
            ghost.appendChild(clone);
            document.documentElement.appendChild(ghost);
            if (typeof ghost.showPopover === 'function') ghost.showPopover();
            window.__cmgRecordingDrag.defaultGhost = ghost;
          }
          source.dispatchEvent(new DragEvent('drag', eventOptions));
          return true;
        })()
        """;

    public static string MoveDrag(ElementPoint point) =>
        $$"""
        (() => {
          const state = window.__cmgRecordingDrag;
          if (!state?.source || !state?.dataTransfer) return false;
          const target = document.elementFromPoint({{Invariant(point.X)}}, {{Invariant(point.Y)}}) || document.body;
          const eventOptions = { bubbles: true, cancelable: true, clientX: {{Invariant(point.X)}}, clientY: {{Invariant(point.Y)}}, dataTransfer: state.dataTransfer };
          if (state.defaultGhost?.isConnected) {
            state.defaultGhost.style.left = `${eventOptions.clientX}px`;
            state.defaultGhost.style.top = `${eventOptions.clientY}px`;
            if (typeof state.defaultGhost.showPopover === 'function') {
              if (state.defaultGhost.matches(':popover-open')) state.defaultGhost.hidePopover();
              state.defaultGhost.showPopover();
            }
          }
          state.source.dispatchEvent(new DragEvent('drag', eventOptions));
          target.dispatchEvent(new DragEvent('dragenter', eventOptions));
          target.dispatchEvent(new DragEvent('dragover', eventOptions));
          return true;
        })()
        """;

    public static string EndDrag(ElementPoint point) =>
        $$"""
        (() => {
          const state = window.__cmgRecordingDrag;
          if (!state?.source || !state?.dataTransfer) return false;
          state.source.dispatchEvent(new DragEvent('dragend', { bubbles: true, cancelable: true, clientX: {{Invariant(point.X)}}, clientY: {{Invariant(point.Y)}}, dataTransfer: state.dataTransfer }));
          if (state.defaultGhost?.matches?.(':popover-open')) state.defaultGhost.hidePopover();
          state.defaultGhost?.remove();
          delete window.__cmgRecordingDrag;
          return true;
        })()
        """;

    public static string JsonString(string value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStringValue(value);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string EscapeTemplate(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("`", "\\`", StringComparison.Ordinal)
            .Replace("${", "\\${", StringComparison.Ordinal);
    }

    private static string Invariant(double value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string MouseEventScript(ElementPoint point, int buttons, string phase) =>
        $$"""
        (() => {
          const x = {{Invariant(point.X)}};
          const y = {{Invariant(point.Y)}};
          const target = document.elementFromPoint(x, y) || document.documentElement || document.body;
          const previous = window.__cmgMouseTarget && window.__cmgMouseTarget.isConnected ? window.__cmgMouseTarget : null;
          const common = { bubbles: true, cancelable: true, composed: true, clientX: x, clientY: y, screenX: x, screenY: y, button: 0, buttons: {{buttons}} };
          const pointer = { ...common, pointerId: 1, pointerType: 'mouse', isPrimary: true };
          const sendPointer = (element, type, options = pointer) => element?.dispatchEvent?.(new PointerEvent(type, options));
          const sendMouse = (element, type, options = common) => element?.dispatchEvent?.(new MouseEvent(type, options));

          if ('{{phase}}' === 'down') {
            window.__cmgMouseTarget = target;
            sendPointer(target, 'pointerover');
            sendPointer(target, 'pointerenter', { ...pointer, bubbles: false });
            sendMouse(target, 'mouseover');
            sendMouse(target, 'mouseenter', { ...common, bubbles: false });
            sendPointer(target, 'pointerdown');
            sendMouse(target, 'mousedown');
            return true;
          }

          if ('{{phase}}' === 'up') {
            sendPointer(target, 'pointerup');
            sendMouse(target, 'mouseup');
            window.__cmgMouseTarget = target;
            return true;
          }

          if (previous !== target) {
            if (previous) {
              sendPointer(previous, 'pointerout');
              sendPointer(previous, 'pointerleave', { ...pointer, bubbles: false });
              sendMouse(previous, 'mouseout');
              sendMouse(previous, 'mouseleave', { ...common, bubbles: false });
            }
            sendPointer(target, 'pointerover');
            sendPointer(target, 'pointerenter', { ...pointer, bubbles: false });
            sendMouse(target, 'mouseover');
            sendMouse(target, 'mouseenter', { ...common, bubbles: false });
          }

          sendPointer(target, 'pointermove');
          sendMouse(target, 'mousemove');
          window.__cmgMouseTarget = target;
          return true;
        })()
        """;
}

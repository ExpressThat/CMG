using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string MoveMouse(ElementPoint point, int buttons) =>
        MouseEventScript(point, buttons, "move");

    public static string MouseDown(ElementPoint point) =>
        MouseEventScript(point, 1, "down");

    public static string MouseUp(ElementPoint point) =>
        MouseEventScript(point, 0, "up");

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

using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string DragAndDrop(string sourceSelector, string targetSelector) =>
        $$"""
        (() => {
          const source = document.querySelector({{JsonString(sourceSelector)}});
          const target = document.querySelector({{JsonString(targetSelector)}});
          if (!source || !target) return false;
          const dataTransfer = new DataTransfer();
          const preserveWritableDataTransferState = propertyName => {
            let value = dataTransfer[propertyName];
            try {
              Object.defineProperty(dataTransfer, propertyName, {
                configurable: true,
                get: () => value,
                set: next => { value = String(next); }
              });
            } catch {}
          };
          preserveWritableDataTransferState('effectAllowed');
          preserveWritableDataTransferState('dropEffect');
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
          const preserveWritableDataTransferState = propertyName => {
            let value = dataTransfer[propertyName];
            try {
              Object.defineProperty(dataTransfer, propertyName, {
                configurable: true,
                get: () => value,
                set: next => { value = String(next); }
              });
            } catch {}
          };
          preserveWritableDataTransferState('effectAllowed');
          preserveWritableDataTransferState('dropEffect');
          let customDragImageSet = false;
          const originalSetDragImage = dataTransfer.setDragImage?.bind(dataTransfer);
          if (originalSetDragImage) {
            try { Object.defineProperty(dataTransfer, 'setDragImage', { configurable: true, value: () => { customDragImageSet = true; } }); } catch {}
          }
          window.__cmgRecordingDrag = { source, dataTransfer, defaultGhost: null };
          const eventOptions = { bubbles: true, cancelable: true, clientX: {{Invariant(point.X)}}, clientY: {{Invariant(point.Y)}}, dataTransfer };
          const mouseOptions = { bubbles: true, cancelable: true, composed: true, clientX: eventOptions.clientX, clientY: eventOptions.clientY, screenX: eventOptions.clientX, screenY: eventOptions.clientY, button: 0, buttons: 1 };
          const pointerOptions = { ...mouseOptions, pointerId: 1, pointerType: 'mouse', isPrimary: true };
          const sendPointer = (element, type, options = pointerOptions) => {
            if (typeof PointerEvent === 'function') element.dispatchEvent(new PointerEvent(type, options));
          };
          const sendMouse = (element, type, options = mouseOptions) => element.dispatchEvent(new MouseEvent(type, options));
          sendPointer(source, 'pointerover');
          sendPointer(source, 'pointerenter', { ...pointerOptions, bubbles: false });
          sendMouse(source, 'mouseover');
          sendMouse(source, 'mouseenter', { ...mouseOptions, bubbles: false });
          sendPointer(source, 'pointermove');
          sendMouse(source, 'mousemove');
          sendPointer(source, 'pointerdown');
          sendMouse(source, 'mousedown');
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
          const mouseOptions = { bubbles: true, cancelable: true, composed: true, clientX: eventOptions.clientX, clientY: eventOptions.clientY, screenX: eventOptions.clientX, screenY: eventOptions.clientY, button: 0, buttons: 1 };
          const pointerOptions = { ...mouseOptions, pointerId: 1, pointerType: 'mouse', isPrimary: true };
          if (typeof PointerEvent === 'function') target.dispatchEvent(new PointerEvent('pointermove', pointerOptions));
          target.dispatchEvent(new MouseEvent('mousemove', mouseOptions));
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
          const target = document.elementFromPoint({{Invariant(point.X)}}, {{Invariant(point.Y)}}) || document.body;
          const eventOptions = { bubbles: true, cancelable: true, clientX: {{Invariant(point.X)}}, clientY: {{Invariant(point.Y)}}, dataTransfer: state.dataTransfer };
          const mouseOptions = { bubbles: true, cancelable: true, composed: true, clientX: eventOptions.clientX, clientY: eventOptions.clientY, screenX: eventOptions.clientX, screenY: eventOptions.clientY, button: 0, buttons: 0 };
          const pointerOptions = { ...mouseOptions, pointerId: 1, pointerType: 'mouse', isPrimary: true };
          if (typeof PointerEvent === 'function') target.dispatchEvent(new PointerEvent('pointerup', pointerOptions));
          target.dispatchEvent(new MouseEvent('mouseup', mouseOptions));
          target.dispatchEvent(new DragEvent('drop', eventOptions));
          state.source.dispatchEvent(new DragEvent('dragend', eventOptions));
          if (state.defaultGhost?.matches?.(':popover-open')) state.defaultGhost.hidePopover();
          state.defaultGhost?.remove();
          delete window.__cmgRecordingDrag;
          return true;
        })()
        """;
}

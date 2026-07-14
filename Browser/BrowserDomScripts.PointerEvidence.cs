using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowGifPointerEvidence(
        ElementPoint point,
        string? selector,
        GifPointerEvidenceOptions options,
        bool focusPulse,
        ElementPoint? teleportOrigin,
        int idlePhase,
        bool useAutomaticContrast) =>
        $$"""
        (() => {
          document.getElementById('__cmg_pointer_evidence')?.remove();
          const cursor = document.getElementById('__cmg_virtual_cursor');
          const point = { x: {{Invariant(point.X)}}, y: {{Invariant(point.Y)}} };
          if (cursor && {{useAutomaticContrast.ToString().ToLowerInvariant()}}) {
            const rgb = value => (value.match(/[\d.]+/g) || []).map(Number);
            let node = document.elementFromPoint(point.x, point.y);
            let channels = [];
            while (node) {
              channels = rgb(getComputedStyle(node).backgroundColor);
              if (channels.length >= 3 && (channels.length < 4 || channels[3] > .08)) break;
              node = node.parentElement;
            }
            const luminance = channels.length >= 3 ? (.2126 * channels[0] + .7152 * channels[1] + .0722 * channels[2]) / 255 : 1;
            const primary = luminance < .52 ? '#ffffff' : '#111827';
            const edge = luminance < .52 ? '#020617' : '#ffffff';
            cursor.querySelectorAll('[data-cmg-pointer-primary]').forEach(part => {
              const attribute = part.dataset.cmgPointerPrimary;
              if (!part.dataset.cmgPointerPrimaryOriginal) part.dataset.cmgPointerPrimaryOriginal = part.getAttribute(attribute) || '';
              part.setAttribute(attribute, primary);
            });
            cursor.querySelectorAll('[data-cmg-pointer-edge]').forEach(part => {
              const attribute = part.dataset.cmgPointerEdge;
              if (!part.dataset.cmgPointerEdgeOriginal) part.dataset.cmgPointerEdgeOriginal = part.getAttribute(attribute) || '';
              part.setAttribute(attribute, edge);
            });
          }
          else if (cursor) {
            cursor.style.removeProperty('--cmg-pointer-color');
            cursor.style.removeProperty('--cmg-pointer-edge');
            cursor.querySelectorAll('[data-cmg-pointer-primary]').forEach(part => {
              if (part.dataset.cmgPointerPrimaryOriginal) part.setAttribute(part.dataset.cmgPointerPrimary, part.dataset.cmgPointerPrimaryOriginal);
            });
            cursor.querySelectorAll('[data-cmg-pointer-edge]').forEach(part => {
              if (part.dataset.cmgPointerEdgeOriginal) part.setAttribute(part.dataset.cmgPointerEdge, part.dataset.cmgPointerEdgeOriginal);
            });
          }
          const selector = {{JsonString(selector ?? string.Empty)}};
          let target = null;
          try { target = selector ? document.querySelector(selector) : null; } catch { }
          const rect = target?.getBoundingClientRect?.();
          const visible = rect && rect.width > 0 && rect.height > 0 && rect.bottom > 0 && rect.right > 0 && rect.top < innerHeight && rect.left < innerWidth;
          const calloutMode = '{{options.TargetCallout.ToString().ToLowerInvariant()}}';
          const tiny = visible && (rect.width < {{options.TargetCalloutThreshold}} || rect.height < {{options.TargetCalloutThreshold}});
          const callout = visible && (calloutMode === 'always' || (calloutMode === 'auto' && tiny));
          const zoomMode = '{{options.TargetZoom.ToString().ToLowerInvariant()}}';
          const zoomTiny = visible && (rect.width < {{options.TargetZoomThreshold}} || rect.height < {{options.TargetZoomThreshold}});
          const targetZoom = visible && (zoomMode === 'always' || (zoomMode === 'auto' && zoomTiny));
          const positionMode = '{{options.PagePosition.ToString().ToLowerInvariant()}}';
          const pageHeight = Math.max(document.documentElement.scrollHeight, document.body?.scrollHeight || 0);
          const pagePosition = positionMode === 'always' || (positionMode === 'auto' && pageHeight > innerHeight * 1.5);
          const origin = {{(teleportOrigin is null ? "null" : $"{{ x: {Invariant(teleportOrigin.X)}, y: {Invariant(teleportOrigin.Y)} }}")}};
          const idlePhase = {{idlePhase}};
          const focused = {{focusPulse.ToString().ToLowerInvariant()}} ? document.activeElement : null;
          const focusRect = focused && focused !== document.body && focused !== document.documentElement ? focused.getBoundingClientRect() : null;
          if (!callout && !targetZoom && !pagePosition && !origin && !idlePhase && !focusRect) return true;
          const overlay = document.createElement('div');
          overlay.id = '__cmg_pointer_evidence';
          overlay.setAttribute('popover', 'manual');
          overlay.style.cssText = 'all:initial;position:fixed;inset:0;width:100vw;height:100vh;margin:0;padding:0;border:0;background:transparent;pointer-events:none;overflow:hidden;z-index:2147483645;';
          const parts = [];
          if (origin) {
            parts.push(`<line x1="${origin.x}" y1="${origin.y}" x2="${point.x}" y2="${point.y}" stroke="#2563eb" stroke-width="2.5" stroke-dasharray="7 6" opacity=".72"/>`);
            parts.push(`<circle cx="${origin.x}" cy="${origin.y}" r="7" fill="#fff" stroke="#2563eb" stroke-width="3"/>`);
          }
          if (callout) {
            const cx = rect.left + rect.width / 2, cy = rect.top + rect.height / 2;
            const bx = Math.max(24, Math.min(innerWidth - 24, rect.right + 32));
            const by = Math.max(24, Math.min(innerHeight - 24, rect.top - 24));
            parts.push(`<rect x="${rect.left - 4}" y="${rect.top - 4}" width="${rect.width + 8}" height="${rect.height + 8}" rx="5" fill="none" stroke="#f59e0b" stroke-width="3"/>`);
            parts.push(`<line x1="${cx}" y1="${cy}" x2="${bx}" y2="${by}" stroke="#111827" stroke-width="5"/><line x1="${cx}" y1="${cy}" x2="${bx}" y2="${by}" stroke="#fbbf24" stroke-width="2.5"/>`);
            parts.push(`<circle cx="${bx}" cy="${by}" r="10" fill="#fbbf24" stroke="#111827" stroke-width="3"/>`);
          }
          if (focusRect?.width > 0 && focusRect?.height > 0) {
            parts.push(`<rect x="${focusRect.left - 6}" y="${focusRect.top - 6}" width="${focusRect.width + 12}" height="${focusRect.height + 12}" rx="7" fill="none" stroke="#0ea5e9" stroke-width="4"/>`);
            parts.push(`<rect x="${focusRect.left - 11}" y="${focusRect.top - 11}" width="${focusRect.width + 22}" height="${focusRect.height + 22}" rx="10" fill="none" stroke="#38bdf8" stroke-width="3" opacity=".42"/>`);
          }
          if (idlePhase) {
            const radius = 18 + idlePhase * 7, opacity = .72 - idlePhase * .14;
            parts.push(`<circle cx="${point.x}" cy="${point.y}" r="${radius}" fill="none" stroke="#2563eb" stroke-width="3" opacity="${opacity}"/>`);
          }
          if (pagePosition) {
            const trackHeight = Math.min(160, Math.max(80, innerHeight * .28));
            const trackY = (innerHeight - trackHeight) / 2;
            const ratio = Math.min(1, innerHeight / Math.max(innerHeight, pageHeight));
            const thumbHeight = Math.max(18, trackHeight * ratio);
            const progress = Math.max(0, Math.min(1, scrollY / Math.max(1, pageHeight - innerHeight)));
            const thumbY = trackY + (trackHeight - thumbHeight) * progress;
            parts.push(`<rect x="${innerWidth - 32}" y="${trackY}" width="7" height="${trackHeight}" rx="3.5" fill="#111827" opacity=".42"/>`);
            parts.push(`<rect x="${innerWidth - 34}" y="${thumbY}" width="11" height="${thumbHeight}" rx="5.5" fill="#fbbf24" stroke="#111827" stroke-width="2"/>`);
          }
          overlay.innerHTML = `<svg width="100%" height="100%" viewBox="0 0 ${innerWidth} ${innerHeight}" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">${parts.join('')}</svg>`;
          if (targetZoom) {
            const inset = document.createElement('div');
            inset.style.cssText = 'all:initial;position:absolute;right:30px;bottom:20px;width:168px;height:112px;overflow:hidden;border:3px solid #fbbf24;border-radius:7px;background:#fff;box-shadow:0 2px 10px #0008;box-sizing:border-box;';
            const stage = document.createElement('div');
            stage.style.cssText = 'all:initial;position:absolute;inset:0;overflow:hidden;';
            const clone = target.cloneNode(true);
            const originals = [target, ...target.querySelectorAll('*')];
            const clones = [clone, ...clone.querySelectorAll('*')];
            originals.forEach((source, index) => {
              const copy = clones[index];
              if (!copy) return;
              const computed = getComputedStyle(source);
              copy.style.cssText = Array.from(computed).map(name => `${name}:${computed.getPropertyValue(name)};`).join('');
              copy.removeAttribute('id');
              if ('value' in source) copy.value = source.value;
              if ('checked' in source) copy.checked = source.checked;
            });
            const scale = Math.max(1, Math.min(4, 136 / rect.width, 78 / rect.height));
            clone.style.position = 'absolute';
            clone.style.margin = '0';
            clone.style.left = `${(168 - rect.width * scale) / 2}px`;
            clone.style.top = `${(112 - rect.height * scale) / 2}px`;
            clone.style.transform = `scale(${scale})`;
            clone.style.transformOrigin = 'top left';
            stage.appendChild(clone);
            inset.appendChild(stage);
            overlay.appendChild(inset);
          }
          document.documentElement.appendChild(overlay);
          if (typeof overlay.showPopover === 'function') overlay.showPopover();
          return true;
        })()
        """;

    public static string RemoveGifPointerEvidence() =>
        "(() => { const node=document.getElementById('__cmg_pointer_evidence'); if(node?.matches?.(':popover-open')) node.hidePopover(); node?.remove(); return true; })()";
}

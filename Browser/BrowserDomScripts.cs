using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string Query(string selector) =>
        $"(window.__cmgQuery?.({JsonString(selector)}) ?? document.querySelector({JsonString(selector)}))";

    public static string QueryAll(string selector) =>
        $"(window.__cmgQueryAll?.({JsonString(selector)}) ?? Array.from(document.querySelectorAll({JsonString(selector)})))";

    public static string ElementAction(string selector, string body) =>
        $$"""
        (() => {
          const element = {{Query(selector)}};
          if (!element) return false;
          {{body}}
        })()
        """;

    public static string ScrollIntoView(string selector) =>
        $"(() => {{ const element = {Query(selector)}; if (!element) return false; element.scrollIntoView({{ block: 'center', inline: 'center' }}); return true; }})()";

    public static string ElementRect(string selector) =>
        $$"""
        (() => {
          const element = {{Query(selector)}};
          if (!element) return null;
          const rect = element.getBoundingClientRect();
          const owns = point => {
            const hit = document.elementFromPoint(point.x, point.y);
            return !!hit && (hit === element || element.contains(hit));
          };
          const quad = element.getBoxQuads?.({ box: 'border' })?.[0];
          const candidates = [];
          if (quad) candidates.push({
            x: (quad.p1.x + quad.p2.x + quad.p3.x + quad.p4.x) / 4,
            y: (quad.p1.y + quad.p2.y + quad.p3.y + quad.p4.y) / 4
          });
          candidates.push(
            { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 },
            { x: rect.left + rect.width * .25, y: rect.top + rect.height * .25 },
            { x: rect.left + rect.width * .75, y: rect.top + rect.height * .25 },
            { x: rect.left + rect.width * .25, y: rect.top + rect.height * .75 },
            { x: rect.left + rect.width * .75, y: rect.top + rect.height * .75 });
          const point = candidates.find(candidate =>
            candidate.x >= 0 && candidate.y >= 0 && candidate.x <= innerWidth && candidate.y <= innerHeight && owns(candidate))
            ?? candidates[0];
          const style = getComputedStyle(element);
          const visual = window.visualViewport;
          return JSON.stringify({
            x: rect.left,
            y: rect.top,
            width: rect.width,
            height: rect.height,
            interactionX: point.x,
            interactionY: point.y,
            devicePixelRatio: window.devicePixelRatio || 1,
            visualScale: visual?.scale || 1,
            visualOffsetX: visual?.offsetLeft || 0,
            visualOffsetY: visual?.offsetTop || 0,
            cssZoom: Number(element.currentCSSZoom || style.zoom || 1) || 1,
            transformed: style.transform !== 'none'
          });
        })()
        """;
}

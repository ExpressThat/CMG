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
          return JSON.stringify({
            x: rect.left,
            y: rect.top,
            width: rect.width,
            height: rect.height
          });
        })()
        """;
}

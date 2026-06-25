using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static partial class BrowserDomScripts
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
}

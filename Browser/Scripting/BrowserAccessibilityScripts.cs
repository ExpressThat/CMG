namespace CMG.Browser.Scripting;

public static class BrowserAccessibilityScripts
{
    public static string Snapshot(string? selector) =>
        $$"""
        (() => {
          const root = {{(string.IsNullOrWhiteSpace(selector) ? "document.body" : $"document.querySelector({Quote(selector)})")}};
          if (!root) throw new Error('No element matched accessibility selector {{Escape(selector ?? "body")}}.');
          const roleOf = element => element.getAttribute('role') || implicitRole(element);
          const nameOf = element => element.getAttribute('aria-label') || element.getAttribute('alt') || element.getAttribute('title') || element.innerText || element.textContent || '';
          const implicitRole = element => {
            const tag = element.tagName;
            if (tag === 'BUTTON') return 'button';
            if (tag === 'A' && element.hasAttribute('href')) return 'link';
            if (tag === 'IMG') return 'img';
            if (tag === 'INPUT' || tag === 'TEXTAREA') return 'textbox';
            if (tag === 'SELECT') return 'combobox';
            if (/^H[1-6]$/.test(tag)) return 'heading';
            return '';
          };
          const nodeOf = element => ({
            role: roleOf(element),
            name: nameOf(element).trim(),
            hidden: element.hidden || element.getAttribute('aria-hidden') === 'true',
            disabled: element.matches(':disabled,[aria-disabled="true"]'),
            children: Array.from(element.children).map(nodeOf).filter(child => child.role || child.name || child.children.length)
          });
          return JSON.stringify(nodeOf(root));
        })()
        """;

    public static string Expect(string role, string name) =>
        $$"""
        (() => {
          const matches = node => {
            const foundRole = node.getAttribute('role') || (node.tagName === 'BUTTON' ? 'button' : node.tagName === 'A' && node.hasAttribute('href') ? 'link' : node.tagName === 'INPUT' || node.tagName === 'TEXTAREA' ? 'textbox' : '');
            const foundName = (node.getAttribute('aria-label') || node.getAttribute('alt') || node.getAttribute('title') || node.innerText || node.textContent || '').trim();
            return foundRole === {{Quote(role)}} && foundName.includes({{Quote(name)}});
          };
          const hit = Array.from(document.querySelectorAll('body *')).find(matches);
          if (!hit) throw new Error('No accessible element matched role={{Escape(role)}} name={{Escape(name)}}.');
          return true;
        })()
        """;

    private static string Quote(string? value) =>
        $"\"{(value ?? string.Empty).Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Escape(string value) =>
        value.Replace("'", "\\'", StringComparison.Ordinal).Replace("`", "\\`", StringComparison.Ordinal).Replace("$", "\\$", StringComparison.Ordinal);
}

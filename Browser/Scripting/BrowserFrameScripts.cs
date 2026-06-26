namespace CMG.Browser.Scripting;

public static class BrowserFrameScripts
{
    public static string TargetCenter(string frameSelector, string selector) =>
        Wrap(frameSelector, $$"""
        const element = resolveFrameElement({{Quote(selector)}});
        if (!element) throw new Error(`No frame element matched selector {{Escape(selector)}}.`);
        const frameRect = frame.getBoundingClientRect();
        const rect = element.getBoundingClientRect();
        return JSON.stringify({ x: frameRect.left + rect.left + rect.width / 2, y: frameRect.top + rect.top + rect.height / 2 });
        """);

    public static string Click(string frameSelector, string selector) =>
        Element(frameSelector, selector, "dispatchMouse(element); return true;");

    public static string Hover(string frameSelector, string selector) =>
        Element(frameSelector, selector, "dispatchMouse(element, false); return true;");

    public static string Type(string frameSelector, string selector, string text) =>
        Element(frameSelector, selector, $"element.focus(); element.value = (element.value || '') + {Quote(text)}; dispatchInput(element); return true;");

    public static string Fill(string frameSelector, string selector, string text) =>
        Element(frameSelector, selector, $"element.focus(); element.value = {Quote(text)}; dispatchInput(element); return true;");

    public static string AssertText(
        string frameSelector,
        string selector,
        string expected,
        string matchMode = "contains",
        bool ignoreCase = false) =>
        Element(frameSelector, selector, $$"""
        const expected = {{Quote(expected)}};
        const matchMode = {{Quote(matchMode)}};
        const ignoreCase = {{ignoreCase.ToString().ToLowerInvariant()}};
        const matchesText = {{TextMatcherScript()}};
        const actual = element.innerText ?? element.textContent ?? '';
        if (!matchesText(actual, expected, matchMode, ignoreCase)) {
          throw new Error(`Expected frame text to match ${expected} using ${matchMode}, got ${actual}.`);
        }
        return true;
        """);

    public static string Evaluate(string frameSelector, string expression) =>
        Wrap(frameSelector, $"return frame.contentWindow.eval({Quote(expression)});");

    public static string WaitForElement(string frameSelector, string selector, int timeout) =>
        Wrap(frameSelector, $$"""
        return new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const element = resolveFrameElement({{Quote(selector)}});
            if (element) { resolve(true); return; }
            if (Date.now() >= deadline) { reject(new Error(`Timed out waiting for frame selector {{Escape(selector)}}.`)); return; }
            setTimeout(poll, 50);
          };
          poll();
        });
        """);

    private static string Element(string frameSelector, string selector, string body) =>
        Wrap(frameSelector, $$"""
        const element = resolveFrameElement({{Quote(selector)}});
        if (!element) throw new Error(`No frame element matched selector {{Escape(selector)}}.`);
        const dispatchInput = target => {
          target.dispatchEvent(new Event('input', { bubbles: true }));
          target.dispatchEvent(new Event('change', { bubbles: true }));
        };
        const dispatchMouse = (target, click = true) => {
          const rect = target.getBoundingClientRect();
          const options = { bubbles: true, cancelable: true, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 };
          target.dispatchEvent(new MouseEvent('mouseover', options));
          target.dispatchEvent(new MouseEvent('mousemove', options));
          if (click) {
            target.dispatchEvent(new MouseEvent('mousedown', options));
            target.dispatchEvent(new MouseEvent('mouseup', options));
            target.dispatchEvent(new MouseEvent('click', options));
          }
        };
        {{body}}
        """);

    private static string Wrap(string frameSelector, string body) =>
        $$"""
        (() => {
          const frame = globalThis.document.querySelector({{Quote(frameSelector)}});
          if (!frame) throw new Error(`No iframe matched selector {{Escape(frameSelector)}}.`);
          const frameDocument = frame.contentDocument;
          if (!frameDocument) throw new Error(`Iframe {{Escape(frameSelector)}} is not same-origin or is not ready.`);
          const document = frameDocument;
          {{LocatorResolverScript()}}
          {{body}}
        })()
        """;

    private static string LocatorResolverScript() =>
        """
        const splitLocator = value => {
          const index = String(value).lastIndexOf('|');
          return index > 0 && index < String(value).length - 1 ? [String(value).slice(0, index), String(value).slice(index + 1)] : null;
        };
        const implicitRole = e => e.tagName === 'BUTTON' ? 'button' : e.tagName === 'A' && e.hasAttribute('href') ? 'link' : e.tagName === 'INPUT' || e.tagName === 'TEXTAREA' ? 'textbox' : '';
        const accessibleName = e => e.getAttribute('aria-label') || e.getAttribute('alt') || e.getAttribute('title') || e.innerText || e.textContent || '';
        const isVisible = e => { const r = e.getBoundingClientRect(); const s = getComputedStyle(e); return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none'; };
        const all = selector => Array.from(document.querySelectorAll(selector));
        const resolveFrameElement = locator => {
          const raw = String(locator || '');
          const equals = raw.indexOf('=');
          if (equals <= 0) return document.querySelector(raw);
          const aliases = { testId: 'testid', 'data-testid': 'testid', getByText: 'text', getByTextExact: 'textExact', getByExactText: 'textExact', getByTextRegex: 'textRegex', getByRole: 'role', getByRoleRegex: 'roleRegex', getByLabel: 'label', getByLabelText: 'label', getByLabelExact: 'labelExact', getByLabelTextExact: 'labelExact', getByLabelRegex: 'labelRegex', getByLabelTextRegex: 'labelRegex', getByPlaceholder: 'placeholder', getByPlaceholderText: 'placeholder', getByPlaceholderExact: 'placeholderExact', getByPlaceholderTextExact: 'placeholderExact', getByPlaceholderRegex: 'placeholderRegex', getByPlaceholderTextRegex: 'placeholderRegex', getByAltText: 'alt', getByAltTextExact: 'altExact', getByAltTextRegex: 'altRegex', getByTitle: 'title', getByTitleExact: 'titleExact', getByTitleRegex: 'titleRegex', getByTestId: 'testid' };
          const key = aliases[raw.slice(0, equals)] || raw.slice(0, equals);
          const value = raw.slice(equals + 1);
          if (key === 'css') return document.querySelector(value);
          if (key === 'testid') return document.querySelector(`[data-testid="${CSS.escape(value)}"]`);
          if (key === 'placeholder') return document.querySelector(`[placeholder="${CSS.escape(value)}"]`);
          if (key === 'alt') return document.querySelector(`[alt="${CSS.escape(value)}"]`);
          if (key === 'title') return document.querySelector(`[title="${CSS.escape(value)}"]`);
          if (key === 'text') return all('body *').filter(e => (e.innerText || e.textContent || '').includes(value)).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0];
          if (key === 'textExact') return all('body *').filter(e => (e.innerText || e.textContent || '').trim() === value).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0];
          if (key === 'textRegex') return all('body *').filter(e => new RegExp(value).test(e.innerText || e.textContent || '')).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0];
          if (key === 'role') { const parts = splitLocator(value); return parts ? all('body *').find(e => (e.getAttribute('role') || implicitRole(e)) === parts[0] && accessibleName(e).includes(parts[1])) : all('body *').find(e => (e.getAttribute('role') || implicitRole(e)) === value); }
          if (key === 'roleRegex') { const parts = splitLocator(value); if (!parts) throw new Error('Locator roleRegex= requires <role>|<name-regex>.'); return all('body *').find(e => (e.getAttribute('role') || implicitRole(e)) === parts[0] && new RegExp(parts[1]).test(accessibleName(e))); }
          if (key === 'label') return all('label').find(e => (e.innerText || '').includes(value))?.control;
          if (key === 'labelExact') return all('label').find(e => (e.innerText || '').trim() === value)?.control;
          if (key === 'labelRegex') return all('label').find(e => new RegExp(value).test(e.innerText || ''))?.control;
          if (key === 'placeholderExact') return all('[placeholder]').find(e => e.getAttribute('placeholder') === value);
          if (key === 'placeholderRegex') return all('[placeholder]').find(e => new RegExp(value).test(e.getAttribute('placeholder') || ''));
          if (key === 'altExact') return all('[alt]').find(e => e.getAttribute('alt') === value);
          if (key === 'altRegex') return all('[alt]').find(e => new RegExp(value).test(e.getAttribute('alt') || ''));
          if (key === 'titleExact') return all('[title]').find(e => e.getAttribute('title') === value);
          if (key === 'titleRegex') return all('[title]').find(e => new RegExp(value).test(e.getAttribute('title') || ''));
          if (key === 'xpath') return document.evaluate(value, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
          if (key === 'first') return document.querySelector(value);
          if (key === 'last') return all(value).at(-1);
          if (key === 'nth') { const parts = splitLocator(value); if (!parts) throw new Error('Locator nth= requires <selector>|<index>.'); return all(parts[0])[Number(parts[1])]; }
          if (key === 'has' || key === 'hasNot') { const parts = splitLocator(value); if (!parts) throw new Error(`Locator ${key}= requires <selector>|<child-selector>.`); return all(parts[0]).find(e => key === 'has' ? e.querySelector(parts[1]) : !e.querySelector(parts[1])); }
          if (key === 'hasText' || key === 'hasNotText') { const parts = splitLocator(value); if (!parts) throw new Error(`Locator ${key}= requires <selector>|<text>.`); return all(parts[0]).find(e => key === 'hasText' ? (e.innerText || e.textContent || '').includes(parts[1]) : !(e.innerText || e.textContent || '').includes(parts[1])); }
          if (key === 'visible') return all(value).find(isVisible);
          if (key === 'or') { const parts = splitLocator(value); return parts ? resolveFrameElement(parts[0]) || resolveFrameElement(parts[1]) : null; }
          if (key === 'and') { const parts = splitLocator(value); return parts ? all(parts[0]).find(e => e.matches(parts[1])) : null; }
          if (key === 'strict') { const matches = all(value); if (matches.length !== 1) throw new Error(`Locator strict= expected 1 match, got ${matches.length}.`); return matches[0]; }
          if (key === 'inside') { const parts = splitLocator(value); return parts ? document.querySelector(parts[0])?.querySelector(parts[1]) : null; }
          if (key === 'closest') { const parts = splitLocator(value); return parts ? document.querySelector(parts[0])?.closest(parts[1]) : null; }
          if (key === 'parent') return document.querySelector(value)?.parentElement;
          if (key === 'next') return document.querySelector(value)?.nextElementSibling;
          if (key === 'previous') return document.querySelector(value)?.previousElementSibling;
          if (key === 'shadow') { const parts = splitLocator(value); return parts ? document.querySelector(parts[0])?.shadowRoot?.querySelector(parts[1]) : null; }
          if (key === 'shadowText') { const parts = splitLocator(value); return parts ? Array.from(document.querySelector(parts[0])?.shadowRoot?.querySelectorAll('*') || []).find(e => (e.innerText || e.textContent || '').includes(parts[1])) : null; }
          return document.querySelector(raw);
        };
        """;

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Escape(string value) =>
        value.Replace("`", "\\`", StringComparison.Ordinal).Replace("$", "\\$", StringComparison.Ordinal);

    private static string TextMatcherScript() =>
        """
        (value, pattern, mode, ignoreCase) => {
          const actual = ignoreCase ? String(value || '').toLowerCase() : String(value || '');
          const expected = ignoreCase ? String(pattern || '').toLowerCase() : String(pattern || '');
          if (mode === 'exact') return actual === expected;
          if (mode === 'regex') return new RegExp(String(pattern || ''), ignoreCase ? 'i' : '').test(String(value || ''));
          return actual.includes(expected);
        }
        """;
}

namespace CMG.Browser.Scripting;

public static class BrowserFrameScripts
{
    public static string TargetCenter(string frameSelector, string selector) =>
        Wrap(frameSelector, $$"""
        const element = document.querySelector({{Quote(selector)}});
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

    public static string AssertText(string frameSelector, string selector, string expected) =>
        Element(frameSelector, selector, $"const actual = element.innerText ?? element.textContent ?? ''; if (!actual.includes({Quote(expected)})) throw new Error(`Expected frame text to contain {Escape(expected)}, got ${{actual}}.`); return true;");

    public static string Evaluate(string frameSelector, string expression) =>
        Wrap(frameSelector, $"return frame.contentWindow.eval({Quote(expression)});");

    public static string WaitForElement(string frameSelector, string selector, int timeout) =>
        Wrap(frameSelector, $$"""
        return new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const element = document.querySelector({{Quote(selector)}});
            if (element) { resolve(true); return; }
            if (Date.now() >= deadline) { reject(new Error(`Timed out waiting for frame selector {{Escape(selector)}}.`)); return; }
            setTimeout(poll, 50);
          };
          poll();
        });
        """);

    private static string Element(string frameSelector, string selector, string body) =>
        Wrap(frameSelector, $$"""
        const element = document.querySelector({{Quote(selector)}});
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
          {{body}}
        })()
        """;

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Escape(string value) =>
        value.Replace("`", "\\`", StringComparison.Ordinal).Replace("$", "\\$", StringComparison.Ordinal);
}

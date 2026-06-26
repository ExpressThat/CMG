# `browser control page runtime`

Element getters, selector evaluation, and page setup commands.

```powershell
cmg browser control page runtime [command] [options]
```

## Subcommands

- [`textContent`](textContent.md): Read element `textContent`.
- [`innerText`](innerText.md): Read element `innerText`.
- [`inputValue`](inputValue.md): Read an input-like element value.
- [`getAttribute`](getAttribute.md): Read an element attribute.
- [`count`](count.md): Count matching elements.
- [`locatorCount`](locatorCount.md): Count matching elements.
- [`boundingBox`](boundingBox.md): Read an element bounding box.
- [`allTextContents`](allTextContents.md): Read `textContent` for all matching elements.
- [`allInnerTexts`](allInnerTexts.md): Read `innerText` for all matching elements.
- [`evaluateOnSelector`](evaluateOnSelector.md): Evaluate JavaScript with one selected element.
- [`evalOnSelector`](evalOnSelector.md): Evaluate JavaScript with one selected element.
- [`evaluateAll`](evaluateAll.md): Evaluate JavaScript with all matching elements.
- [`evalAll`](evalAll.md): Evaluate JavaScript with all matching elements.
- [`addInitScript`](addInitScript.md): Register JavaScript for future documents.
- [`evaluateOnNewDocument`](evaluateOnNewDocument.md): Register JavaScript for future documents.
- [`addScriptTag`](addScriptTag.md): Inject a script tag.
- [`addStyleTag`](addStyleTag.md): Inject a style tag or stylesheet link.
- [`exposeFunction`](exposeFunction.md): Expose a deterministic page-side function.
- [`exposeBinding`](exposeBinding.md): Expose a deterministic page-side binding.

## Behavior

- Requires a browser started with [`browser launch`](../../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and parseable action output lines to stdout.
- Writes browser, selector, file, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control page runtime count ".row"
cmg browser control page runtime boundingBox "#card"
cmg browser control page runtime allTextContents ".item"
```

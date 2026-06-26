# `browser control capture`

Element and page capture command group.

```powershell
cmg browser control capture [command] [options]
```

## Subcommands

- [`getElement`](getElement.md): Return HTML or a screenshot for a selected element.
- [`html`](html.md): Print an element's outer HTML.
- [`screenshot`](screenshot.md): Capture an element screenshot.
- [`screenshotPage`](screenshotPage.md): Capture a viewport or full-page screenshot.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- `getElement` uses the direct element capture service.
- `html`, `screenshot`, and `screenshotPage` run the same underlying scripting actions as `browser control script`.
- Writes capture output to stdout.
- Writes browser, selector, capture, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control capture getElement "h1" --html
cmg browser control capture html "#profileDialog"
cmg browser control capture screenshot "#profileDialog" --output profile-dialog.png
cmg browser control capture screenshotPage --full-page --output page.png
```

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
- [`printPdf`](printPdf.md): Print the current page to PDF.
- [`pdf`](pdf.md): Print the current page to PDF.
- [`expectScreenshot`](expectScreenshot.md): Compare an element or page screenshot to a baseline.
- [`toHaveScreenshot`](toHaveScreenshot.md): Compare an element or page screenshot to a baseline.

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
cmg browser control capture screenshot "#profileDialog" --mask "#clock;#ad" --output masked-dialog.png
cmg browser control capture screenshotPage --animations disabled --caret hide --output deterministic-page.png
cmg browser control capture screenshotPage --full-page --output page.png
cmg browser control capture printPdf --path page.pdf
cmg browser control capture pdf --path page.pdf
cmg browser control capture expectScreenshot "#profileDialog" --baseline baselines/profile-dialog.png --output actual.png
cmg browser control capture toHaveScreenshot "#profileDialog" --baseline baselines/profile-dialog.png --output actual.png
```

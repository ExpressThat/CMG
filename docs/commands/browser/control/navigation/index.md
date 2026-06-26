# `browser control navigation`

Navigation and page state command group.

```powershell
cmg browser control navigation [command] [options]
```

## Subcommands

- [`navigate`](navigate.md): Navigate the primary page target.
- [`reload`](reload.md): Reload the primary page target.
- [`goBack`](goBack.md): Navigate one step back in page history.
- [`goForward`](goForward.md): Navigate one step forward in page history.
- [`waitForUrl`](waitForUrl.md): Wait until the current URL contains text.
- [`expectUrl`](expectUrl.md): Assert that the current URL contains text.
- [`expectTitle`](expectTitle.md): Assert that the current page title contains text.
- [`toHaveURL`](toHaveURL.md): Assert that the current URL contains text.
- [`toHaveTitle`](toHaveTitle.md): Assert that the current page title contains text.
- [`waitForLoadState`](waitForLoadState.md): Wait until the page reaches a load state.
- [`waitForNavigation`](waitForNavigation.md): Wait until navigation reaches a state.
- [`url`](url.md): Print the current page URL.
- [`title`](title.md): Print the current page title.
- [`content`](content.md): Print the current page HTML.
- [`setContent`](setContent.md): Replace the current page HTML.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and action output lines to stdout.
- Writes browser, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control navigation navigate "https://example.com"
cmg browser control navigation reload
cmg browser control navigation title
cmg browser control navigation expectTitle "Checkout"
cmg browser control navigation toHaveURL "checkout"
cmg browser control navigation waitForLoadState complete
cmg browser control navigation waitForNavigation "checkout" --wait-until domcontentloaded
```

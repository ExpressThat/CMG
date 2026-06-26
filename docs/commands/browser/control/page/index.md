# `browser control page`

Page evaluation, viewport, and utility command group.

```powershell
cmg browser control page [command] [options]
```

## Subcommands

- [`evaluate`](evaluate.md): Evaluate JavaScript in the primary page.
- [`setViewport`](setViewport.md): Set viewport dimensions.
- [`viewport`](viewport.md): Set viewport dimensions.
- [`setViewportSize`](setViewportSize.md): Set viewport dimensions.
- [`showMessageBar`](showMessageBar.md): Inject or update a fixed message bar at the top of the page.
- [`caption`](caption.md): Alias for `showMessageBar` used for recording narration.
- [`highlight`](highlight.md): Draw a temporary visual highlight around an element.
- [`delay`](delay.md): Pause for a duration.
- [`runtime`](runtime/index.md): Element getters, selector evaluation, and page setup commands.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and action output lines to stdout.
- Writes browser, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control page evaluate "document.title"
cmg browser control page setViewport --width 1280 --height 720
cmg browser control page viewport --width 390 --height 844 --mobile --touch
cmg browser control page showMessageBar "Working"
cmg browser control page caption "Opening menu"
cmg browser control page highlight "getByRole=button|Save" --message Save
cmg browser control page delay 250
cmg browser control page runtime textContent "h1"
```

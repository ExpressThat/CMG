# `browser control wait`

Page synchronization wait command group.

```powershell
cmg browser control wait [command] [options]
```

## Subcommands

- [`element`](element.md): Wait until an element exists.
- [`selector`](selector.md): Wait until a selector exists and print a selector wait line.
- [`function`](function.md): Wait until a JavaScript expression becomes truthy.
- [`timeout`](timeout.md): Wait for a fixed duration.
- [`waitForElement`](waitForElement.md): Exact scripting alias for `element`.
- [`waitForSelector`](waitForSelector.md): Exact scripting alias for `selector`.
- [`waitForFunction`](waitForFunction.md): Exact scripting alias for `function`.
- [`waitForTimeout`](waitForTimeout.md): Exact scripting alias for `timeout`.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` output lines to stdout when waits succeed.
- Some wait actions also write parseable detail lines, such as `SELECTOR`, `FUNCTION`, or `WAIT_TIMEOUT`.
- Writes browser, selector, JavaScript, timeout, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control wait element "#ready" --timeout 5000
cmg browser control wait selector "text=Saved"
cmg browser control wait function "window.appReady === true" --timeout 10000
cmg browser control wait timeout 250
cmg browser control wait waitForSelector "text=Saved"
```

# `browser control tabs`

Tab and popup target command group.

```powershell
cmg browser control tabs [command] [options]
```

## Subcommands

- [`list`](list.md): List available page targets.
- [`listTabs`](listTabs.md): List available page targets.
- [`open`](open.md): Open a new tab.
- [`openTab`](openTab.md): Open a new tab.
- [`wait`](wait.md): Wait until at least this many tabs exist.
- [`waitForTab`](waitForTab.md): Wait until at least this many tabs exist.
- [`waitForPopup`](waitForPopup.md): Wait until at least this many tabs or popups exist.
- [`activate`](activate.md): Activate a tab by index.
- [`activateTab`](activateTab.md): Activate a tab by index.
- [`close`](close.md): Close a tab by index.
- [`closeTab`](closeTab.md): Close a tab by index.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and tab output lines to stdout.
- Writes browser, timeout, or invalid-index errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control tabs list
cmg browser control tabs listTabs
cmg browser control tabs open "https://example.com"
cmg browser control tabs openTab "https://example.com"
cmg browser control tabs wait --count 2
cmg browser control tabs waitForPopup --count 2
cmg browser control tabs activate --index 1
cmg browser control tabs close --index 1
```

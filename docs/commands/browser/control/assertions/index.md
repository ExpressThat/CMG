# `browser control assertions`

Page and element assertion command group.

```powershell
cmg browser control assertions [command] [options]
```

## Subcommands

- [`assertText`](assertText.md): Assert that an element contains text.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` output lines to stdout when assertions pass.
- Writes assertion, browser, selector, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control assertions assertText "h1" "Ready"
```

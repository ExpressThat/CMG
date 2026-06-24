# `browser control waitForElement`

Runs the scripting `waitForElement` action once from the command line.

```powershell
cmg browser control waitForElement "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector.

## Options

- `--timeout <milliseconds>`: Optional timeout. Default is `5000`.

## Stdout

```text
PASS 001 waitForElement #openProfileDialog
```

## Stderr

Writes a clear error when the selector does not appear before the timeout.

## Exit Codes

- `0`: Element appeared.
- `1`: Browser is not running, selector did not appear, or the action failed.

## Example

```powershell
cmg browser control waitForElement "#openProfileDialog" --timeout 5000
```

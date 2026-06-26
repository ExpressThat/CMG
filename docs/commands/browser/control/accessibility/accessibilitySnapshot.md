# `browser control accessibility accessibilitySnapshot`

Runs the scripting `accessibilitySnapshot` action once from the command line.

```powershell
cmg browser control accessibility accessibilitySnapshot [selector] [--output <file>]
```

## Arguments

- `[selector]`: Optional CSS selector to snapshot. Defaults to `document.body`.

## Options

- `--output <file>`: Write snapshot JSON to this file instead of stdout.

## Stdout

Without `--output`, stdout includes the JSON payload:

```text
PASS 001 accessibilitySnapshot
ACCESSIBILITY 001 {"role":"","name":"Ready",...}
```

With `--output`, stdout includes the written file path.

## Stderr

Writes browser, selector, accessibility, parse, or action errors.

## Exit Codes

- `0`: Snapshot was created.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control accessibility accessibilitySnapshot "#dialog"
```

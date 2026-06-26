# `browser control accessibility snapshot`

Creates an accessibility snapshot.

```powershell
cmg browser control accessibility snapshot [selector] [--output <file>]
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

With `--output`, stdout includes the written file path:

```text
ACCESSIBILITY 001 C:\path\a11y.json
```

## Exit Codes

- `0`: Snapshot was created.
- `1`: Browser is not running or the action failed.

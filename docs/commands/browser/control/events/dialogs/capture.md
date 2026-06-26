# `browser control events dialogs capture`

Installs browser dialog capture with default `accept` behavior.

```powershell
cmg browser control events dialogs capture [--prompt-text <text>]
```

## Options

- `--prompt-text <text>`: Text returned from accepted prompts.

## Stdout

```text
PASS 001 captureDialogs
DIALOG_CAPTURE 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Dialog capture was installed.
- `1`: Browser is not running or the action failed.

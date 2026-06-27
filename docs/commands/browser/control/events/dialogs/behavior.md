# `browser control events dialogs behavior`

Sets automated browser dialog behavior.

```powershell
cmg browser control events dialogs behavior <behavior> [--prompt-text <text>]
```

## Arguments

- `<behavior>`: `accept` or `dismiss`.

## Options

- `--prompt-text <text>`: Text returned from accepted prompts.

## Stdout

```text
PASS 001 setDialogBehavior accept
DIALOG_BEHAVIOR 001 accept
```

## Stderr

Writes browser, argument, or action errors.

## Exit Codes

- `0`: Dialog behavior was set.
- `1`: Browser is not running or the action failed.

# `browser control events dialogs handleDialog`

Runs the scripting `handleDialog` alias once from the command line.

```powershell
cmg browser control events dialogs handleDialog <behavior> [--prompt-text <text>]
```

## Arguments

- `<behavior>`: Dialog behavior: `accept` or `dismiss`.

## Options

- `--prompt-text <text>`: Prompt text to return when accepting prompts.

## Stdout

```text
PASS 001 handleDialog dismiss
DIALOG_BEHAVIOR 001 dismiss
```

## Stderr

Writes browser, argument, parse, or action errors.

## Exit Codes

- `0`: Dialog behavior was set.
- `1`: Browser is not running, arguments are invalid, or the action failed.

## Examples

```powershell
cmg browser control events dialogs handleDialog dismiss
```

# `browser control events dialogs captureDialogs`

Runs the scripting `captureDialogs` action once from the command line.

```powershell
cmg browser control events dialogs captureDialogs [--prompt-text <text>]
```

## Options

- `--prompt-text <text>`: Prompt text to return when accepting prompts.

## Stdout

```text
PASS 001 captureDialogs
DIALOG_CAPTURE 001 accept
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Dialog capture was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control events dialogs captureDialogs --prompt-text "CMG"
```

# `browser control events console captureConsole`

Runs the scripting `captureConsole` action once from the command line.

```powershell
cmg browser control events console captureConsole
```

## Stdout

```text
PASS 001 captureConsole
CONSOLE_CAPTURE 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Console capture was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control events console captureConsole
```

# `browser control events console capture`

Installs page-side console capture.

```powershell
cmg browser control events console capture
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

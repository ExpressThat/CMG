# `browser control events console capture`

Deprecated compatibility alias for [`captureConsole`](captureConsole.md). Ensures page-side console diagnostics capture is installed and preserves any existing `window.__cmgConsole` entries. CMG normally arms diagnostics automatically on launch or attach.

```powershell
cmg browser control events console capture
```

## Stdout

```text
PASS 001 line=1 action=captureConsole
CONSOLE_CAPTURE 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Console capture was installed or was already installed.
- `1`: Browser is not running or the action failed.

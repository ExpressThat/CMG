# `browser control events console captureConsole`

Runs the scripting `captureConsole` action once from the command line.

Deprecated: CMG now arms console diagnostics automatically when it launches or attaches to a controlled browser/app. This command remains as an idempotent compatibility alias that ensures capture is installed and preserves any existing `window.__cmgConsole` entries.

```powershell
cmg browser control events console captureConsole
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

## Examples

```powershell
cmg browser control events console captureConsole
```

# `browser control events pageErrors capturePageErrors`

Runs the scripting `capturePageErrors` action once from the command line.

Deprecated: CMG now arms page-error diagnostics automatically when it launches or attaches to a controlled browser/app. This command remains as an idempotent compatibility alias that ensures capture is installed and preserves any existing `window.__cmgPageErrors` entries.

```powershell
cmg browser control events pageErrors capturePageErrors
```

## Stdout

```text
PASS 001 line=1 action=capturePageErrors
PAGE_ERROR_CAPTURE 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Page-error capture was installed or was already installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control events pageErrors capturePageErrors
```

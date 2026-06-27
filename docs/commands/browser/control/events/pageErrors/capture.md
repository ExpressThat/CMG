# `browser control events pageErrors capture`

Deprecated compatibility alias for [`capturePageErrors`](capturePageErrors.md). Ensures page-side error diagnostics capture is installed and preserves any existing `window.__cmgPageErrors` entries. CMG normally arms diagnostics automatically on launch or attach.

```powershell
cmg browser control events pageErrors capture
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

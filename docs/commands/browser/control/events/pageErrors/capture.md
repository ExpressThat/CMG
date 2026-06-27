# `browser control events pageErrors capture`

Installs page-side error capture.

```powershell
cmg browser control events pageErrors capture
```

## Stdout

```text
PASS 001 capturePageErrors
PAGE_ERROR_CAPTURE 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Page error capture was installed.
- `1`: Browser is not running or the action failed.

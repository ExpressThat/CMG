# `browser control events pageErrors capturePageErrors`

Runs the scripting `capturePageErrors` action once from the command line.

```powershell
cmg browser control events pageErrors capturePageErrors
```

## Stdout

```text
PASS 001 capturePageErrors
PAGE_ERRORS_CAPTURE 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Page error capture was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control events pageErrors capturePageErrors
```

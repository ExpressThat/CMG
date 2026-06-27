# `browser control context reset`

Clears context state and navigates the current page to `about:blank`.

```powershell
cmg browser control context reset
```

## Stdout

```text
PASS 001 resetContext
CONTEXT_RESET 001
```

## Exit Codes

- `0`: Context state was reset.
- `1`: Browser is not running or the action failed.

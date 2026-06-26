# `browser control context serviceWorkers`

Allows or blocks service worker registration.

```powershell
cmg browser control context serviceWorkers <mode>
```

## Arguments

- `<mode>`: `allow` or `block`.

## Stdout

```text
PASS 001 serviceWorkers block
SERVICE_WORKERS 001 block
```

## Exit Codes

- `0`: Service worker mode was updated.
- `1`: Browser is not running or the action failed.

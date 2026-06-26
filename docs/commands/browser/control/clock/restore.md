# `browser control clock restore`

Restores native page clock APIs.

```powershell
cmg browser control clock restore
```

## Stdout

```text
PASS 001 restoreClock
CLOCK_RESTORED 001
```

## Exit Codes

- `0`: Native clock APIs were restored.
- `1`: Browser is not running or the action failed.

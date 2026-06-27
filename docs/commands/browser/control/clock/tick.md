# `browser control clock tick`

Advances deterministic page-side time.

```powershell
cmg browser control clock tick <milliseconds>
```

## Arguments

- `<milliseconds>`: Non-negative milliseconds to advance.

## Stdout

```text
PASS 001 tick 250
TICK 001 250 now=1700000000250
```

## Exit Codes

- `0`: Clock was advanced.
- `1`: Browser is not running or the action failed.

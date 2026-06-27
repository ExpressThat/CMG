# `browser control page runtime inputValue`

Reads an input-like element value.

```powershell
cmg browser control page runtime inputValue "<selector>"
```

## Stdout

```text
PASS 001 inputValue #email
VALUE 001 user@example.test
```

## Exit Codes

- `0`: Value was read.
- `1`: Browser is not running or the action failed.

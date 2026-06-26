# `browser control storage local`

Runs the scripting `localStorage` action once from the command line.

```powershell
cmg browser control storage local <operation> [key] [value]
```

## Arguments

- `<operation>`: `get`, `set`, `remove`, or `clear`.
- `[key]`: Required for `get`, `set`, and `remove`.
- `[value]`: Required for `set`.

## Stdout

```text
PASS 001 localStorage set token abc
LOCAL_STORAGE 001 set token
```

For `get`:

```text
PASS 001 localStorage get token
LOCAL_STORAGE 001 get token abc
```

## Stderr

Writes browser or argument errors.

## Exit Codes

- `0`: The storage operation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control storage local set token abc
```

# `browser control storage session`

Runs the scripting `sessionStorage` action once from the command line.

```powershell
cmg browser control storage session <operation> [key] [value]
```

## Arguments

- `<operation>`: `get`, `set`, `remove`, or `clear`.
- `[key]`: Required for `get`, `set`, and `remove`.
- `[value]`: Required for `set`.

## Stdout

```text
PASS 001 sessionStorage set token abc
SESSION_STORAGE 001 set token
```

For `get`:

```text
PASS 001 sessionStorage get token
SESSION_STORAGE 001 get token abc
```

## Stderr

Writes browser or argument errors.

## Exit Codes

- `0`: The storage operation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control storage session clear
```

# `browser control storage cookie`

Runs the scripting `cookie` action once from the command line.

```powershell
cmg browser control storage cookie <operation> [key] [value]
```

## Arguments

- `<operation>`: `get`, `set`, `remove`, or `clear`.
- `[key]`: Required for `set` and `remove`; optional for `get`.
- `[value]`: Required for `set`.

## Stdout

```text
PASS 001 cookie set mode demo
COOKIE 001 set mode
```

For all cookies:

```text
PASS 001 cookie get
COOKIE 001 get mode=demo
```

## Stderr

Writes browser or argument errors.

## Exit Codes

- `0`: The cookie operation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control storage cookie set mode demo
```

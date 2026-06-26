# `browser control page runtime exposeFunction`

Exposes a deterministic page-side function.

```powershell
cmg browser control page runtime exposeFunction <name> "<expression>"
```

## Arguments

- `<name>`: JavaScript identifier to install on `window`.
- `<expression>`: JavaScript function expression.

## Stdout

```text
PASS 001 exposeFunction cmgAdd (a, b) => a + b
EXPOSED_FUNCTION 001 cmgAdd
```

## Exit Codes

- `0`: Function was exposed.
- `1`: Browser is not running or the action failed.

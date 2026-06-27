# `browser control page runtime exposeBinding`

Exposes a deterministic page-side binding.

```powershell
cmg browser control page runtime exposeBinding <name> "<expression>"
```

## Arguments

- `<name>`: JavaScript identifier to install on `window`.
- `<expression>`: JavaScript function expression. CMG passes a source object as the first argument.

## Stdout

```text
PASS 001 exposeBinding cmgBinding (source, value) => value
EXPOSED_FUNCTION 001 cmgBinding
```

## Exit Codes

- `0`: Binding was exposed.
- `1`: Browser is not running or the action failed.

# `browser control page runtime textContent`

Reads an element's `textContent`.

```powershell
cmg browser control page runtime textContent "<selector>"
```

## Stdout

```text
PASS 001 textContent h1
TEXT 001 Heading
```

## Exit Codes

- `0`: Text was read.
- `1`: Browser is not running or the action failed.

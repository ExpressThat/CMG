# `browser control page runtime innerText`

Reads an element's `innerText`.

```powershell
cmg browser control page runtime innerText "<selector>"
```

## Stdout

```text
PASS 001 innerText h1
TEXT 001 Heading
```

## Exit Codes

- `0`: Text was read.
- `1`: Browser is not running or the action failed.

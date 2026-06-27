# `browser control context browserContexts close`

Closes a browser context by id or target id.

```powershell
cmg browser control context browserContexts close <id>
```

## Arguments

- `<id>`: Context id or target id.

## Stdout

```text
PASS 001 closeContext context-1
CONTEXT_CLOSED 001 context-1
```

## Exit Codes

- `0`: Context was closed.
- `1`: Browser is not running or the action failed.

# `browser control context browserContexts use`

Activates a browser context by id or target id.

```powershell
cmg browser control context browserContexts use <id>
```

## Arguments

- `<id>`: Context id or target id.

## Stdout

```text
PASS 001 useContext context-1
CONTEXT_ACTIVE 001 context-1
```

## Exit Codes

- `0`: Context was activated.
- `1`: Browser is not running or the action failed.

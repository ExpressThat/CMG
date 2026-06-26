# `browser control context clear`

Clears storage, cookies, caches, IndexedDB, and service workers in the current page context.

```powershell
cmg browser control context clear
```

## Stdout

```text
PASS 001 clearContext
CONTEXT_CLEARED 001
```

## Exit Codes

- `0`: Context state was cleared.
- `1`: Browser is not running or the action failed.

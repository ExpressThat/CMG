# `browser control context browserContexts list`

Lists browser contexts.

```powershell
cmg browser control context browserContexts list
```

## Stdout

```text
PASS 001 listContexts
CONTEXT 0 id=<context-id> target=<target-id> active=true url="about:blank"
```

## Exit Codes

- `0`: Contexts were listed.
- `1`: Browser is not running or the action failed.

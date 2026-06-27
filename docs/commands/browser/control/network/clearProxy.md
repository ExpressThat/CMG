# `browser control network clearProxy`

Runs the scripting `clearProxy` action once from the command line.

```powershell
cmg browser control network clearProxy
```

## Stdout

```text
PASS 001 clearProxy
PROXY_CLEARED 001
```

## Exit Codes

- `0`: Proxy rewrite was cleared.
- `1`: Browser is not running or the action failed.

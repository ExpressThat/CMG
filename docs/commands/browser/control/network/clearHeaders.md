# `browser control network clearHeaders`

Runs the scripting `clearExtraHTTPHeaders` action once from the command line.

```powershell
cmg browser control network clearHeaders
```

## Stdout

```text
PASS 001 clearExtraHTTPHeaders
HEADERS_CLEARED 001
```

## Exit Codes

- `0`: Extra headers were cleared.
- `1`: Browser is not running or the action failed.

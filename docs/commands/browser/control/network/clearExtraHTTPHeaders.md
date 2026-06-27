# `browser control network clearExtraHTTPHeaders`

Runs the scripting `clearExtraHTTPHeaders` action once from the command line.

```powershell
cmg browser control network clearExtraHTTPHeaders
```

## Stdout

```text
PASS 001 clearExtraHTTPHeaders
HEADERS_CLEARED 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Extra HTTP headers were cleared.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network clearExtraHTTPHeaders
```

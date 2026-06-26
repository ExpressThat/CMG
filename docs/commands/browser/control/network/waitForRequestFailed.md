# `browser control network waitForRequestFailed`

Runs the scripting `waitForRequestFailed` action once from the command line.

```powershell
cmg browser control network waitForRequestFailed "<pattern>" [options]
```

Options and failure behavior match [`waitForRequest`](waitForRequest.md).

## Stdout

```text
PASS 001 waitForRequestFailed /api/down
REQUEST_FAILED 001 {"url":"/api/down","error":"Failed to fetch"}
```

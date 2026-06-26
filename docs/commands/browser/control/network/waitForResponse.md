# `browser control network waitForResponse`

Runs the scripting `waitForResponse` action once from the command line.

```powershell
cmg browser control network waitForResponse "<pattern>" [options]
```

Options and failure behavior match [`waitForRequest`](waitForRequest.md).

## Stdout

```text
PASS 001 waitForResponse /api/profile
RESPONSE 001 {"url":"/api/profile","status":200}
```

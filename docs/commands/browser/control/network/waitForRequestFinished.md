# `browser control network waitForRequestFinished`

Runs the scripting `waitForRequestFinished` action once from the command line.

```powershell
cmg browser control network waitForRequestFinished "<pattern>" [options]
```

Options and failure behavior match [`waitForRequest`](waitForRequest.md). Header filters match response headers.

## Stdout

```text
PASS 001 waitForRequestFinished /api/profile
REQUEST_FINISHED 001 {"url":"/api/profile","status":200}
```

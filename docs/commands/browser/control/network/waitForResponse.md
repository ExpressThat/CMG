# `browser control network waitForResponse`

Runs the scripting `waitForResponse` action once from the command line.

```powershell
cmg browser control network waitForResponse "<pattern>" [options]
```

Options and failure behavior match [`waitForRequest`](waitForRequest.md). Header filters match response headers for responses and finished requests.

## Stdout

```text
PASS 001 waitForResponse /api/profile
RESPONSE 001 {"url":"/api/profile","status":200}
```

## Example

```powershell
cmg browser control network waitForResponse "/api/profile" --status 200 --header "Content-Type: json"
```

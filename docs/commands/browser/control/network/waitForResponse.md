# `browser control network waitForResponse`

Runs the scripting `waitForResponse` action once from the command line.

```powershell
cmg browser control network waitForResponse "<pattern>" [options]
```

Options and failure behavior match [`waitForRequest`](waitForRequest.md), including `--match contains|exact|regex` and `--ignore-case` for URL matching. Header filters match response headers for responses and finished requests.

## Stdout

```text
PASS 001 waitForResponse /api/profile
RESPONSE 001 {"url":"/api/profile","status":200}
```

## Example

```powershell
cmg browser control network waitForResponse "/api/profile" --status 200 --header "Content-Type: json"
cmg browser control network waitForResponse "/api/profile/\d+" --match regex --ignore-case
```

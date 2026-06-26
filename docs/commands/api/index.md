# `api`

HTTP API utility commands.

```powershell
cmg api [command] [options]
```

## Subcommands

- [`request`](request.md): Send an HTTP request.

## Behavior

- Does not require a browser.
- Uses the same API request runner as the scripting `apiRequest` action.
- Writes `API` and `API_BODY` lines to stdout when a response is received, or `API_BODY_FILE` when `--output` writes the response body to disk.
- Writes request, timeout, validation, or parse errors to stderr.
- Exits `0` on success and `1` on failure.

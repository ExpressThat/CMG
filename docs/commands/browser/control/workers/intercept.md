# `browser control workers intercept`

Patches worker `fetch()` responses.

```powershell
cmg browser control workers intercept "<pattern>" [options]
```

## Arguments

- `<pattern>`: Worker fetch URL substring to intercept.

## Options

- `--status <status>`: Mocked response status. Default is `200`.
- `--body <text>`: Mocked response body.
- `--content-type <type>`: Mocked response content type. Default is `text/plain`.
- `--target <id-or-url>`: Worker id or URL substring. Defaults to the first worker.

## Stdout

```text
PASS 001 workerIntercept /api/profile status=200
WORKER_INTERCEPT 001 routes=1 /api/profile
```

## Exit Codes

- `0`: Worker fetch interception was installed.
- `1`: Browser is not running or the action failed.

# `browser control workers workerIntercept`

Runs the scripting `workerIntercept` action once from the command line.

```powershell
cmg browser control workers workerIntercept "<pattern>" [options]
```

## Arguments

- `<pattern>`: Worker fetch URL substring to intercept.

## Options

- `--status <status>`: Mocked response status. Default is `200`.
- `--body <body>`: Mocked response body.
- `--content-type <type>`: Mocked response content type. Default is `text/plain`.
- `--target <id-or-url>`: Worker id or URL substring. Defaults to the first worker.

## Stdout

```text
PASS 001 workerIntercept /api
WORKER_INTERCEPT 001 /api
```

## Stderr

Writes browser, worker, parse, or action errors.

## Exit Codes

- `0`: Worker fetch interception was installed.
- `1`: Browser is not running, the worker is missing, or the action failed.

## Examples

```powershell
cmg browser control workers workerIntercept "/api/profile" --status 200 --body "{}"
```

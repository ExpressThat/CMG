# `browser control workers wait`

Waits for a matching worker target.

```powershell
cmg browser control workers wait "<pattern>" [--timeout <milliseconds>]
```

## Arguments

- `<pattern>`: Worker URL substring to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 waitForWorker worker.js timeout=5000
WORKER_READY 001 id=<id> url="worker.js"
```

## Exit Codes

- `0`: Worker was found.
- `1`: Browser is not running or the action failed.

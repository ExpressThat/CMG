# `browser control workers wait`

Waits for a matching worker target.

```powershell
cmg browser control workers wait "<pattern>" [options]
```

## Arguments

- `<pattern>`: Worker URL text to match. Default matching is case-insensitive substring matching unless `--match` changes it.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.
- `--match <contains|exact|regex>`: Worker URL match mode. Default is `contains`.
- `--ignore-case`: Match the worker URL without case sensitivity.

## Stdout

```text
PASS 001 waitForWorker worker.js timeout=5000
WORKER_READY 001 id=<id> url="worker.js"
```

## Exit Codes

- `0`: Worker was found.
- `1`: Browser is not running or the action failed.

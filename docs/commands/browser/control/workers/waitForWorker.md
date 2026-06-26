# `browser control workers waitForWorker`

Runs the scripting `waitForWorker` action once from the command line.

```powershell
cmg browser control workers waitForWorker "<pattern>" [--timeout <milliseconds>]
```

## Arguments

- `<pattern>`: Worker URL substring to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 waitForWorker worker.js
WORKER_READY 001 worker-1 https://example.test/worker.js
```

## Stderr

Writes browser, worker, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching worker appeared.
- `1`: Browser is not running or the wait timed out.

## Examples

```powershell
cmg browser control workers waitForWorker "worker.js" --timeout 5000
```

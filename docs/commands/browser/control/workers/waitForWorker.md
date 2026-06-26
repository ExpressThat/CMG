# `browser control workers waitForWorker`

Runs the scripting `waitForWorker` action once from the command line.

```powershell
cmg browser control workers waitForWorker "<pattern>" [options]
```

## Arguments

- `<pattern>`: Worker URL text to match. Default matching is case-insensitive substring matching unless `--match` changes it.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.
- `--match <contains|exact|regex>`: Worker URL match mode. Default is `contains`.
- `--ignore-case`: Match the worker URL without case sensitivity.

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

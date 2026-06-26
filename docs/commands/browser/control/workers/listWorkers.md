# `browser control workers listWorkers`

Runs the scripting `listWorkers` action once from the command line.

```powershell
cmg browser control workers listWorkers
```

## Stdout

```text
PASS 001 listWorkers
WORKER 001 worker-1 https://example.test/worker.js
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Worker targets were listed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control workers listWorkers
```

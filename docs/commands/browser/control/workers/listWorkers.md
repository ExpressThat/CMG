# `browser control workers listWorkers`

Runs the scripting `listWorkers` action once from the command line.

```powershell
cmg browser control workers listWorkers
```

## Stdout

```text
PASS 001 listWorkers
WORKER 0 id=<id> type=worker title="worker.js" url="https://example.test/worker.js"
```

## Behavior

This command initializes CMG's page-side worker bridge. Same-origin classic workers created after this command runs keep CMG-owned id, URL, and worker name/title metadata so later `evaluate`, `workerEvaluate`, `intercept`, and `workerIntercept` commands can target them reliably in headless Chrome. Workers that already existed before initialization are listed from Chrome target metadata when available.

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Worker targets were listed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control workers listWorkers
```

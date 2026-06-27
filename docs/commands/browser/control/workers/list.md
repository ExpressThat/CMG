# `browser control workers list`

Lists worker targets.

```powershell
cmg browser control workers list
```

## Stdout

```text
PASS 001 listWorkers
WORKER 0 id=<id> type=worker title="" url="worker.js"
```

## Behavior

This command initializes CMG's page-side worker bridge. Same-origin classic workers created after this command runs keep CMG-owned id, URL, and worker name/title metadata so later `evaluate`, `workerEvaluate`, `intercept`, and `workerIntercept` commands can target them reliably in headless Chrome. Workers that already existed before initialization are listed from Chrome target metadata when available.

## Exit Codes

- `0`: Workers were listed.
- `1`: Browser is not running or the action failed.

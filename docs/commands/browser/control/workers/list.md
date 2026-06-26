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

## Exit Codes

- `0`: Workers were listed.
- `1`: Browser is not running or the action failed.

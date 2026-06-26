# `browser control workers evaluate`

Evaluates JavaScript in a worker target.

```powershell
cmg browser control workers evaluate "<expression>" [--target <id-or-url>]
```

## Arguments

- `<expression>`: JavaScript expression evaluated in the worker.

## Options

- `--target <id-or-url>`: Worker id or URL substring. Defaults to the first worker.

## Stdout

```text
PASS 001 workerEvaluate self.location.href
WORKER_EVALUATE 001 https://example.test/worker.js
```

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running or the action failed.

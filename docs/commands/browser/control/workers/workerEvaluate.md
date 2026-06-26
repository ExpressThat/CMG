# `browser control workers workerEvaluate`

Runs the scripting `workerEvaluate` action once from the command line.

```powershell
cmg browser control workers workerEvaluate "<expression>" [--target <id-or-url>]
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

## Stderr

Writes browser, worker, JavaScript, parse, or action errors.

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running, the worker is missing, or evaluation failed.

## Examples

```powershell
cmg browser control workers workerEvaluate "self.location.href" --target worker.js
```

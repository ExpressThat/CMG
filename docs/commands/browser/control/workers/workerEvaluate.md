# `browser control workers workerEvaluate`

Runs the scripting `workerEvaluate` action once from the command line.

```powershell
cmg browser control workers workerEvaluate "<expression>" [--target <id-or-url>]
```

## Arguments

- `<expression>`: JavaScript expression evaluated in the worker.

## Options

- `--target <id-or-url>`: Worker id or URL substring. Defaults to the first worker.

## Behavior

This command initializes CMG's page-side worker bridge before it evaluates. Same-origin classic workers created after worker support is initialized can be matched by id, URL substring, or worker name/title. Workers that already existed before initialization may fall back to Chrome target metadata, which can omit URL/title details in headless flows.

## Stdout

```text
PASS 001 workerEvaluate self.ready === true
WORKER_EVALUATE 001 true
```

## Stderr

Writes browser, worker, JavaScript, parse, or action errors.

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running, the worker is missing, or evaluation failed.

## Examples

```powershell
cmg browser control workers list
cmg browser control runtime evaluate "window.worker = new Worker('/worker.js', { name: 'worker.js' }); true"
cmg browser control workers workerEvaluate "self.ready === true" --target worker.js
```

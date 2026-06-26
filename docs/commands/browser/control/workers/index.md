# `browser control workers`

Worker inspection, evaluation, and interception commands.

```powershell
cmg browser control workers [command] [options]
```

## Subcommands

- [`list`](list.md): List worker targets.
- [`listWorkers`](listWorkers.md): List worker targets.
- [`wait`](wait.md): Wait for a matching worker target.
- [`waitForWorker`](waitForWorker.md): Wait for a matching worker target.
- [`evaluate`](evaluate.md): Evaluate JavaScript in a worker target.
- [`workerEvaluate`](workerEvaluate.md): Evaluate JavaScript in a worker target.
- [`intercept`](intercept.md): Patch worker fetch responses.
- [`workerIntercept`](workerIntercept.md): Patch worker fetch responses.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS`, `WORKER`, `WORKER_READY`, `WORKER_EVALUATE`, or `WORKER_INTERCEPT` lines to stdout.
- Writes browser, worker, timeout, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control workers workerIntercept "/api/profile/\d+" --match regex --ignore-case --body-file fixtures/profile.json --header "X-Trace: worker"
```

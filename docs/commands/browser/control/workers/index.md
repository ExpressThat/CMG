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
- `list`, `evaluate`, and `intercept` initialize CMG's page-side worker bridge. Same-origin classic workers created after that initialization can be matched by id, URL substring, or worker name/title and can be evaluated or intercepted in real Chrome headless runs.
- Workers that already existed before bridge initialization are still listed from browser target metadata when Chrome exposes them, but URL/title metadata and direct evaluation depend on what the browser target reports.
- Worker interception patches the matched worker's `fetch()` function. It does not rewrite browser-level navigation requests, service worker traffic, module workers, or cross-origin workers.

## Examples

```powershell
cmg browser control workers list
cmg browser control runtime evaluate "window.worker = new Worker('/worker.js', { name: 'worker.js' }); true"
cmg browser control workers evaluate "self.ready === true" --target worker.js
cmg browser control workers workerIntercept "/api/profile/\d+" --match regex --ignore-case --body-file fixtures/profile.json --header "X-Trace: worker"
```

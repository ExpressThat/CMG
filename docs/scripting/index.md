# CMG Browser-Control Scripting

Browser-control scripts are for agents and people who need to drive the browser directly. They use the `.cmgscript` extension and are executed with:

```powershell
cmg browser control script --file flow.cmgscript
cmg --edge browser control script --file flow.cmgscript
cmg --firefox browser control script --file flow.cmgscript
```

Use browser-control scripts when a single selector command is not enough and an agent needs to describe a repeatable browser flow. This is separate from `cmg run`, which executes the new test DSL and intentionally rejects V1 flat scripts.

Feature parity actions are available in both script types unless a command page says otherwise:

- `browser control script` is the direct browser-control surface for agents.
- `cmg run` is the structured test DSL with suites, hooks, reports, retries, sharding, traces, and optional per-test GIFs.
- Shared actions include pointer-aware browser actions, browser contexts, worker control, storage state, API requests, network fetch mocks, fixtures and file assertions, PDF output, file upload, tab/popup controls, and visual assertions.

## Guides

- [Syntax](syntax.md)
- [Actions](actions.md)
- [Examples](examples.md)
- [Errors](errors.md)
- [GIF Recording](gif-recording.md)
- [Migration To The New DSL](migration.md)

Runnable examples live in [`../../demo-scripts/`](../../demo-scripts/).

## Minimal Script

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
click "#openProfileDialog"
```

## Execution Model

- Scripts run one line at a time.
- Blank lines and comment lines are ignored.
- Variables are expanded before each action runs.
- Execution stops on the first failure.
- Successful execution exits `0`; failure exits `1`.

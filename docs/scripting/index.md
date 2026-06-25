# CMG Scripting

CMG scripts use the `.cmgscript` extension and are executed with:

```powershell
cmg browser control script --file flow.cmgscript
cmg --edge browser control script --file flow.cmgscript
cmg --firefox browser control script --file flow.cmgscript
```

Use scripts when a single selector command is not enough and an agent needs to describe a repeatable browser flow.

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

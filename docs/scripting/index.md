# CMG Browser-Control Scripting

Browser-control scripts are for agents and people who need to drive the browser directly. They use the `.cmgscript` extension and are executed with:

```powershell
cmg browser control script --file flow.cmgscript
cmg --edge browser control script --file flow.cmgscript
cmg --firefox browser control script --file flow.cmgscript
cmg browser control validateScript --file flow.cmgscript
```

Use browser-control scripts when a single selector command is not enough and an agent needs to describe a repeatable browser flow. Use `cmg run` when the same actions should be planned and reported as tests.

The same action surface is available in both script types unless a command page says otherwise:

- `browser control script` is the direct browser-control surface for agents.
- `cmg run` is the structured test DSL with suites, hooks, reports, retries, sharding, traces, and optional per-test GIFs.
- Runner-only declarations include `test.each`, `it.each`, and `specify.each` for data-driven tests.
- Runner declaration metadata such as `owner=`, `issue=`, and `annotation.<type>=...` is written into reports.
- Shared actions are grouped in the [action index](action-index.md), with full syntax and failure behavior in the [action reference](actions.md).
- Shared language features include imports, variables, `set` block capture, conditionals, loops, selector iteration, scoped macros, and nested blocks.

If you are new to CMG, read this guide in order:

1. [Quick Start](../quick-start.md): make a GIF, run a test, and use CMG from an agent.
2. [Script Vs Runner](script-vs-runner.md): choose the right execution mode.
3. [Style Guide](style-guide.md): write scripts that stay readable as they grow.
4. [Action Index](action-index.md): find the action family you need.
5. [Detailed Action Reference](actions.md): look up exact options, output, and failure behavior.

## Guides

- [Quick Start](../quick-start.md)
- [Script Vs Runner](script-vs-runner.md)
- [Syntax](syntax.md)
- [Action Index](action-index.md)
- [Detailed Action Reference](actions.md)
- [GIF Recording](gif-recording.md)
- [Style Guide](style-guide.md)
- [Examples](examples.md)
- [Cookbook Reference](cookbook-reference.md)
- [Errors](errors.md)
- [Migration Guide](migration.md)

Runnable examples live in [`../../demo-scripts/`](../../demo-scripts/).

## Minimal Script

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
step "Open dialog" {
  click "#openProfileDialog"
}
```

Record it:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif
```

## Minimal Test

```text
test "opens profile dialog" {
  navigate "C:\Projects\CMG\index.html"
  click "#openProfileDialog"
  expectVisible "#profileDialog"
}
```

Run it:

```powershell
cmg run profile-dialog.cmgscript --gif demo-output\gifs
```

## Execution Model

- Scripts run one line at a time.
- Blank lines and comment lines are ignored.
- Variables are expanded before each action runs.
- `browser control validateScript --file <path>` checks imports and syntax without connecting to a browser.
- Execution stops on the first failure.
- Successful execution exits `0`; failure exits `1`.

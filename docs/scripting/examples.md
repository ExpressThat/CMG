# CMG Examples

Use this page as a learning path. It starts with small scripts you can run immediately, then points to deeper examples when you need a specific capability.

For the full catalogue of advanced examples, see the [cookbook reference](cookbook-reference.md). Runnable files live in [`../../demo-scripts/`](../../demo-scripts/).

## Start Here

| Goal | Read | Run |
| --- | --- | --- |
| Make a first GIF | [First GIF](#first-gif) | `demo-scripts\01-dialog-flow.cmgscript` |
| Write a first test | [First Test](#first-test) | `demo-scripts\20-runner-flow.cmgscript` |
| Show pointer behavior | [Visual Evidence](#visual-evidence) | `demo-scripts\10-css-hover-states.cmgscript` |
| Reuse script logic | [Variables And Macros](#variables-and-macros) | `demo-scripts\30-control-flow-macros.cmgscript` |
| Handle failures clearly | [Failure Feedback](#failure-feedback) | `demo-scripts\52-explicit-fail.cmgscript` |

Start the browser before running direct scripts:

```powershell
cmg browser launch
```

When developing from the repository, use `dotnet run --` in place of `cmg`.

## First GIF

Create a short browser-control script:

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000

step "Open the profile dialog" {
  click "#openProfileDialog"
}

type "#profileName" "CMG quick start"
```

Record it:

```powershell
cmg browser control script --file first-gif.cmgscript --gif demo-output\first-gif.gif
```

The recording shows CMG's virtual pointer, real page pointer events, hover states, typed text, captions, and any captured frames before a failure.

## First Test

Use `cmg run` when the same actions should become repeatable tests with reports and optional per-test GIFs:

```text
suite "profile dialog" {
  beforeEach {
    navigate "C:\Projects\CMG\index.html"
    waitForElement "#openProfileDialog"
  }

  test "opens" tag=smoke {
    click "#openProfileDialog"
    expectVisible "#profileDialog"
    expectText "#lastDialogAction" "None"
  }
}
```

Run it with reports:

```powershell
cmg run profile.test.cmgscript --report-html demo-output\report.html --report-json demo-output\report.json
```

Add `--gif demo-output\gifs` when every selected test should produce a whole-test GIF.

## Visual Evidence

Pointer-aware actions move the same virtual pointer that appears in GIFs. This keeps the visible pointer, browser events, hover state, screenshots, drag ghosts, and captions aligned.

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#hoverDemoCard"

step "Show the hover card" {
  hover "#hoverDemoCard"
  expectText "#hoverStateText" "Hover card active"
}
```

Run the complete hover demo:

```powershell
cmg browser control script --file demo-scripts\10-css-hover-states.cmgscript --gif demo-output\css-hover-states.gif
```

For drag evidence, run:

```powershell
cmg browser control script --file demo-scripts\07-complex-drag-flow.cmgscript --gif demo-output\complex-drag.gif
```

## Variables And Macros

Use `set` for values and action output. The variable stores the actual payload value, not a pass/fail wrapper:

```text
set title {
  evaluate "document.title"
}

if (${title} contains "CMG") {
  caption "Loaded ${title}"
}

expect (${title} contains "CMG")
expect evaluate "document.title" contains "CMG"
softExpect (${title} contains "Dashboard") message="Dashboard title was not shown"
```

Macros are reusable, scoped blocks:

```text
macro openProfile name {
  click "#openProfileDialog"
  fill "#profileName" "${name}"
  return "${name}"
}

set savedName {
  call openProfile "Ada"
}

set labels {
  allTextContents ".command"
}

foreachJson label "${labels}" {
  caption "Command ${index}: ${label}"
}
```

Variables set inside a macro are local to that macro call. A macro can read variables from the parent tree where it was defined, and local values shadow parent values without mutating them.

## Failure Feedback

Assertions and explicit failures explain what broke. Use `expect` or `assert` for generic conditions, `softExpect` when later diagnostics should still run, `skip` when the flow is not applicable, and `fail` when a script has its own reason to abort:

```text
expect (${savedName} != "") message="Profile name was not saved"
softExpect evaluate "window.optionalPanelReady" == "true" message="Optional panel did not load"

if (${featureEnabled} == false) {
  skip "Feature flag disabled"
}

try {
  fail "Expected optional panel"
} catch error {
  caption "${error}"
}
```

Runner failures include `STEP FAIL` diagnostics on stderr and report entries with the line, action, and reason.

## Common Next Steps

| Need | Where To Go |
| --- | --- |
| Choose direct scripts or `cmg run` | [Script vs Runner](script-vs-runner.md) |
| Find an action family quickly | [Action Index](action-index.md) |
| Learn syntax, logic, loops, and macros | [Syntax](syntax.md) |
| Understand GIF blocks and `--gif` | [GIF Recording](gif-recording.md) |
| Write maintainable scripts | [Style Guide](style-guide.md) |
| Browse every example pattern | [Cookbook Reference](cookbook-reference.md) |

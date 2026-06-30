# CMG Examples

Use this page as a learning path. It starts with small scripts you can run immediately, then points to deeper examples when you need a specific capability.

For the full catalogue of advanced examples, see the [cookbook reference](cookbook-reference.md). Runnable files live in [`../../demo-scripts/`](../../demo-scripts/).

## Start Here

| Goal | Read | Run |
| --- | --- | --- |
| Make a first GIF | [First GIF](#first-gif) | `demo-scripts\01-dialog-flow.cmgscript` |
| Write a first test | [First Test](#first-test) | `demo-scripts\20-runner-flow.cmgscript` |
| Show pointer behavior | [Visual Evidence](#visual-evidence) | `demo-scripts\10-css-hover-states.cmgscript` |
| Tune GIF quality | [Visual Evidence](#visual-evidence) | `demo-scripts\148-gif-quality.cmgscript` |
| Choreograph GIF pointer movement | [Visual Evidence](#visual-evidence) | `demo-scripts\149-gif-pointer-choreography.cmgscript` |
| Reuse script logic | [Variables And Macros](#variables-and-macros) | `demo-scripts\30-control-flow-macros.cmgscript` |
| Handle failures clearly | [Failure Feedback](#failure-feedback) | `demo-scripts\52-explicit-fail.cmgscript` |
| Tune one slow section | [Scoped Timeouts](#scoped-timeouts) | `demo-scripts\134-scoped-timeouts.cmgscript` |
| Run the same test for data rows | [Parameterized Tests](#parameterized-tests) | `demo-scripts\136-parameterized-tests.cmgscript` |
| Add report metadata | [Report Annotations](#report-annotations) | `demo-scripts\138-report-annotations.cmgscript` |
| Parameterize scripts from outside | [Initial Variables](#initial-variables) | `demo-scripts\139-cli-variables.cmgscript` |
| Use relative navigation | [Base URL](#base-url) | `demo-scripts\141-base-url.cmgscript` |

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
cmg browser control script --file first-gif.cmgscript --gif demo-output\first-gif.gif --gif-quality highest
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

Use a runner config when CI or an agent should reuse the same reports, traces, retries, variables, and selection defaults:

```powershell
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --list
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --var mode=cli
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --project chrome-smoke
```

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
cmg browser control script --file demo-scripts\10-css-hover-states.cmgscript --gif demo-output\css-hover-states.gif --gif-quality highest
```

For drag evidence, run:

```powershell
cmg browser control script --file demo-scripts\07-complex-drag-flow.cmgscript --gif demo-output\complex-drag.gif
```

To compare GIF encoder presets and the block aliases, run:

```powershell
cmg browser control script --file demo-scripts\148-gif-quality.cmgscript
```

The `gif`, `recordVideo`, and `screencast` blocks all use the same CMG recorder. `quality=highest` is the default and gives the most color-faithful palette; `high`, `medium`, and `low` trade fidelity for smaller/faster artifacts.

To choreograph pointer timing and drag evidence, run:

```powershell
cmg browser control script --file demo-scripts\149-gif-pointer-choreography.cmgscript --gif demo-output\pointer-choreography.gif --pointer-duration 500 --gif-hold-after-action 700
cmg browser control script --file demo-scripts\150-gif-failure-hold.cmgscript --gif demo-output\failure-hold.gif --gif-hold-on-failure 1800 --gif-timeline demo-output\timelines
```

Recording blocks can set `pointerDuration=`, `pointerSpeed=`, `pointerEasing=`, `clickPulse=`, `holdAfterAction=`, `holdOnFailure=`, and `timeline=` as defaults. Use `recording { ... }` or `withRecording { ... }` when several actions or nested recording blocks should share the same defaults without starting a recording by themselves. Use `pauseGif <milliseconds>` for recording-only holds that make the artifact easier to read without sleeping the browser, and `recordCheckpoint "name"` for named JSON timeline markers. If a block has child actions, such as `dragAndDrop { ... }`, the parent options are scoped defaults and each child action can override them locally. Use `--gif-timeline` or block-level `timeline=true` when reports or agents need JSON timing metadata beside the GIF.

For stable screenshot evidence, mask volatile regions only in the artifact. The GIF still shows the real page and pointer choreography:

```text
setContent "<main><h1>Evidence</h1><p id='clock'>12:34:56</p><button id='save'>Save</button></main>"
screenshotPage output="demo-output\masked-evidence.png" mask="#clock" maskColor="#000000" animations=disabled caret=hide
```

Run the complete screenshot mask demos:

```powershell
cmg browser control script --file demo-scripts\143-screenshot-mask.cmgscript
cmg run demo-scripts\144-screenshot-mask-runner.cmgscript
cmg browser control script --file demo-scripts\145-screenshot-deterministic.cmgscript
cmg run demo-scripts\146-screenshot-deterministic-runner.cmgscript
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

## Initial Variables

Use command-line variables when an agent, CI job, or wrapper needs to supply data without editing the script:

```powershell
cmg browser control script --file demo-scripts\139-cli-variables.cmgscript --var user=Ada
cmg run demo-scripts\140-runner-variables.cmgscript --env tenant=demo
```

Use runner declaration variables when the value belongs with the suite or test:

```text
describe "tenant flow" var.tenant=demo {
  test "uses default tenant" {
    expect (${tenant} == "demo")
  }

  test "overrides tenant" var.tenant=staging {
    expect (${tenant} == "staging")
  }
}
```

Declaration variables are inserted before macros and hooks, so helper macros can read them. Test-level values override suite-level values, and explicit `set` actions can still change values later in the current script scope.

## Base URL

Use `--base-url` when scripts should navigate relative to an app root:

```powershell
cmg browser control script --file demo-scripts\141-base-url.cmgscript --base-url https://example.test/app/
cmg run demo-scripts\142-base-url-runner.cmgscript --base-url https://example.test/app/
```

Runner suites and tests can declare their own base URL:

```text
describe "app" baseUrl="https://example.test/app/" {
  test "opens profile" {
    navigate "profile"
  }
}
```

Base URL resolution affects `navigate`, `goto`, `visit`, `openTab`, and `newContext url=`. It does not move the virtual pointer by itself; pointer-aware actions after navigation keep their normal GIF behavior.

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

## Scoped Timeouts

Use scoped timeout blocks when one slow section needs longer waits without changing the rest of the script:

```text
withTimeout 10000 {
  waitForSelector "#slow-panel"
}

withTimeout default=5000 navigation=15000 assertion=2000 {
  navigate "C:\Projects\CMG\index.html" waitUntil=load
  expectText "#status" "Ready"
}
```

The old timeout defaults are restored after the block, even if a child action fails and a surrounding `try` catches it. The block itself is non-visual; pointer-aware child actions still record through CMG's normal virtual pointer path.

## Parameterized Tests

Use `test.each`, `it.each`, or `specify.each` when one runner test should execute once per data row:

```text
test.each "opens ${page}" as=page values="profile,checkout" tag=smoke {
  click "#${page}"
}

test.each "opens ${case.name}" as=case json="[{\"name\":\"Profile\",\"selector\":\"#profile\"}]" {
  click "${case.selector}"
}
```

Expanded tests are ordinary scheduled tests. They work with `--list`, `--grep`, `--tag`, retries, sharding, reports, traces, and per-test GIF recording.

## Report Annotations

Use runner declaration metadata when a report should explain ownership, issues, links, requirements, or notes:

```text
describe "checkout" owner=qa annotation.requirement="REQ-1" {
  test "submits payment" issue="BUG-7" link="https://example.test/BUG-7" {
    click "#pay"
  }
}
```

Annotations are report-only metadata. They appear in JSON, HTML, and JUnit reports and do not change browser execution or GIF recording.

## Common Next Steps

| Need | Where To Go |
| --- | --- |
| Choose direct scripts or `cmg run` | [Script vs Runner](script-vs-runner.md) |
| Find an action family quickly | [Action Index](action-index.md) |
| Learn syntax, logic, loops, and macros | [Syntax](syntax.md) |
| Understand GIF blocks and `--gif` | [GIF Recording](gif-recording.md) |
| Write maintainable scripts | [Style Guide](style-guide.md) |
| Browse every example pattern | [Cookbook Reference](cookbook-reference.md) |

# Script Vs Runner

CMG has two `.cmgscript` execution modes because browser automation has two common jobs: direct control and repeatable tests.

| Need | Use | Why |
| --- | --- | --- |
| Drive a browser from an agent or one-off flow | `cmg browser control script --file flow.cmgscript` | Runs actions directly in order, stops on first failure, and can write one GIF or one trace for the whole script. |
| Run checks as a test suite | `cmg run tests` | Adds suites, tests, hooks, tags, grep, retries, sharding, reports, traces, and per-test GIFs. |
| Create a quick visual demo | `browser control script --gif demo.gif` | Fastest path from script to shareable pointer-accurate GIF. |
| Produce CI-style evidence | `cmg run --report-html --report-json --gif artifacts\gifs` | Gives parseable run output, structured reports, and visual evidence per test. |
| Let an AI inspect or manipulate a page | `browser control script`, one-shot `browser control` commands, or both | Direct scripts are compact, parseable, and do not require generating a JavaScript harness. |

## What Both Modes Share

- The same action syntax.
- The same rich locators and provider-style `getBy*` locators.
- The same virtual pointer behavior for pointer-aware actions.
- The same `gif`, `recordVideo`, and `screencast` block behavior.
- Imports, variables, `set` capture, conditionals, loops, selector iteration, scoped macros, `try`/`catch`/`finally`, `within`, and frame blocks.
- Explicit failures through `fail "reason"`.

## Direct Browser-Control Scripts

Direct scripts can start with browser actions:

```text
navigate "C:\Projects\CMG\index.html"
click "#openProfileDialog"
expectVisible "#profileDialog"
```

Run them with:

```powershell
cmg browser control script --file flow.cmgscript --gif flow.gif
```

Use direct scripts for agent-controlled exploration, bug reproduction, product demos, visual QA, and short automation journeys.

## Structured Runner Tests

Runner files organize actions into tests:

```text
suite "profile dialog" {
  test "opens" {
    navigate "C:\Projects\CMG\index.html"
    click "#openProfileDialog"
    expectVisible "#profileDialog"
  }
}
```

Run them with:

```powershell
cmg run profile-dialog.cmgscript --report-html report.html --gif gifs
```

Use the runner for repeatable test suites, PR evidence, smoke checks, retries, sharding, reports, and traces.

## Moving A Direct Script Into The Runner

If a file intended for `cmg run` starts with a top-level browser action, wrap it in a `test` block. If it is meant to stay a direct browser-control script, keep running it with `browser control script --file`.

See the [migration guide](migration.md) for examples.

# Migration Guide

CMG has one current DSL with two execution shapes:

- Direct scripts run browser actions in order with `cmg browser control script`.
- Runner files organize actions into tests with `cmg run`.

Use this guide when a direct script should become a structured runner test, or when `cmg run` reports that a file starts with an action at the top level.

If a runner file starts with an action such as `navigate`, `click`, or `assertText`, `cmg run` fails with a migration error. Top-level runner entries must be `test`/`it`/`specify`, `suite`/`describe`/`context`, `beforeAll`/`before`, `afterAll`/`after`, `beforeEach`, `afterEach`, or `macro`.

## Basic Script

Before:

```text
navigate "https://example.com"
click "#open"
assertText "#status" "Ready"
```

After:

```text
test "basic flow" {
  navigate "https://example.com"
  click "#open"
  assertText "#status" "Ready"
}
```

## Captions

Before:

```text
showMessageBar "Opening dialog"
click "#open"
```

After:

```text
test "dialog" {
  step "Opening dialog" {
    click "#open"
  }
}
```

`caption "message"` is also available when only a caption is needed.

## GIF Recording

Command-level recording captures each entire test:

```powershell
cmg run flow.cmgscript --gif artifacts\gifs
```

Script-level recording captures only a block when command-level `--gif` is not used:

```text
test "partial recording" {
  navigate "https://example.com"
  gif "open-dialog" {
    click "#open"
    assertText "#dialog" "Profile"
  }
}
```

If `cmg run --gif` is used, script-level `gif` blocks do not create nested GIFs. Their actions are recorded as part of the whole-test GIF.

## Fill

Before:

```text
clear "#name"
type "#name" "CMG"
```

After:

```text
fill "#name" "CMG"
```

`fill` still uses CMG's visual clear-and-type path when recorded, so the virtual pointer interacts with the field.

## Suites And Hooks

```text
beforeEach {
  navigate "https://example.com"
}

suite "profile" {
  test "opens dialog" {
    click "#open"
  }
}
```

## Direct Script Actions In Tests

Direct browser-control actions remain available inside tests, including navigation, waits, pointer actions, assertions, runtime evaluation, capture, tabs, frames, network, storage, contexts, workers, files, variables, macros, and visual `gif` blocks. Use the [action index](action-index.md) for the compact map and [actions](actions.md) for detailed syntax.

Unsupported actions fail explicitly rather than silently doing nothing.

## Reports And Failure Feedback

Use runner report options to capture diagnostics:

```powershell
cmg run flow.cmgscript --report-json artifacts\flow.json --report-html artifacts\flow.html --report-junit artifacts\flow.xml
```

Reports include test status, stdout lines, GIF paths, and step failure reasons. CLI stderr includes `STEP FAIL` lines for failed steps.

# Migration To The New CMG DSL

The new CMG DSL replaces the original flat `.cmgscript` format. Existing scripts should be wrapped into tests and, when needed, visual `gif` blocks.

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

## Existing Commands

Most original action names remain available inside tests: `navigate`, `waitForElement`, `click`, `type`, `clear`, `press`, `hover`, `scrollIntoView`, `select`, `delay`, `html`, `screenshot`, `screenshotPage`, `assertText`, `evaluate`, `setViewport`, `dragAndDrop`, `listTabs`, `activateTab`, `closeTab`, `set`, and `moveMouse`.

Features planned for full Cypress, Puppeteer, and Playwright parity fail explicitly until implemented rather than silently doing nothing.

## Reports And Failure Feedback

Use runner report options to capture diagnostics:

```powershell
cmg run flow.cmgscript --report-json artifacts\flow.json --report-html artifacts\flow.html --report-junit artifacts\flow.xml
```

Reports include test status, stdout lines, GIF paths, and step failure reasons. CLI stderr includes `STEP FAIL` lines for failed steps.

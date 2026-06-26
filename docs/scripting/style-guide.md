# CMG Script Style Guide

CMG scripts should read like a short explanation of the browser journey. Optimize for scripts that agents can modify safely and people can review quickly.

## Prefer Clear Evidence

Use `step` around moments that should be understood in a GIF:

```text
step "Open profile dialog" {
  click "#openProfileDialog"
  waitForVisible "#profileDialog"
}
```

Use `gif "name" { ... }` for a focused recording when the command is not already running with `--gif`. Use command-level `--gif` when you want the entire direct script or test recorded.

## Keep Tests Small

One test should prove one behavior. Put setup in hooks or macros:

```text
suite "profile dialog" {
  beforeEach {
    navigate "C:\Projects\CMG\index.html"
    waitForElement "#openProfileDialog"
  }

  test "opens" {
    click "#openProfileDialog"
    expectVisible "#profileDialog"
  }
}
```

## Name Things For Humans

Use test, step, macro, and variable names that explain intent:

```text
set saveButton "getByRole=button|Save"

macro saveProfile name {
  fill "getByLabel=Profile name" "${name}"
  click "${saveButton}"
}
```

Avoid names that only describe implementation details, such as `clickButton1`.

## Use Stable Locators

Prefer user-facing or test-owned locators:

```text
click "getByRole=button|Save"
fill "getByLabel=Email" "agent@example.com"
click "getByTestId=submit-profile"
```

Use CSS selectors when they are stable and meaningful. Avoid brittle selectors that depend on generated class names or deep layout.

## Capture Values Deliberately

Use `set name { action }` when you need the action payload, not the log line:

```text
set pageTitle {
  title
}

if (${pageTitle} contains "Checkout") {
  caption "Checkout loaded"
}
```

Variables set inside macros are local to that macro call. Macros can read variables from the parent tree where they were defined, and local variables shadow parent variables without mutating them.

## Make Failure Reasons Useful

Prefer assertions that explain the expected state:

```text
expectText "#status" "Saved" timeout=5000
expectNoConsole level=error timeout=1000
waitForResponse "/api/profile" status=200 timeout=5000
```

Use `fail "message"` inside control flow when the script has enough context to explain the problem.
Use `skip "reason"` when the current environment makes the flow intentionally not applicable, such as a missing feature flag or account capability.

Use generic `expect` / `assert` when the condition is about values or action output rather than one element state:

```text
expect (${mode} in "checkout" "billing") message="Unsupported checkout mode"
expect evaluate "window.appReady" == "true"
```

Use `softExpect` sparingly for diagnostic sweeps where the rest of the script should still collect screenshots, logs, or GIF evidence:

```text
softExpect evaluate "window.optionalPanelReady" == "true" message="Optional panel was not ready"
```

## Keep GIFs Watchable

Use captions or steps for non-visual work:

```text
step "Prepare authenticated state" {
  storageState path="demo-output\auth.json"
}
```

Avoid recording long idle periods. Prefer explicit waits without large delays, and use `caption` for state changes that cannot move the pointer.

## Structure Reuse With Macros

Macros are best for repeated browser journeys, not for hiding every single action:

```text
macro login email {
  fill "getByLabel=Email" "${email}"
  click "getByRole=button|Continue"
  expectVisible "#dashboard"
}
```

Nested macros, loops, conditionals, `try`/`catch`, `within`, `frame`, and `gif` blocks are supported. Keep nesting readable; extract a macro when a block becomes hard to scan.

## Prefer A Linear Story First

Write the simplest readable journey before adding abstraction:

- Start with a linear script or test.
- Add a macro when the same journey repeats.
- Use `foreachJson` for JSON array output from actions such as `allTextContents`; use `foreachList` for small delimited value sets.
- Add `retry` or `toPass` around genuinely flaky waits or eventually-consistent checks.
- Use `withTimeout` around one slow section instead of permanently changing defaults for the rest of the script.
- Use `fail "reason"` for guard clauses when the script can explain the problem better than a generic assertion.
- Avoid deeply nested control flow unless the nesting mirrors the user journey. Split a large branch into a macro when it becomes hard to scan.

## Format For Diffability

- Put one action per line unless a tiny inline expression is clearer.
- Indent blocks by two spaces.
- Quote values that contain spaces.
- Keep comments short and useful.
- Prefer `timeout=` on waits and assertions where page timing matters.

Dense spacing is accepted by the parser, but tidy scripts are easier for agents and people to edit safely.

# `browser control wait auto`

Runs the scripting `wait` alias once from the command line.

```powershell
cmg browser control wait auto "<target>" [--timeout <milliseconds>]
```

The CLI uses `auto` because `wait` is already the command group name. It maps to the scripting `wait` action:

- If `<target>` is an integer, CMG pauses for that many milliseconds.
- Otherwise, CMG treats `<target>` as a selector and waits for an element.

Use [`waitForTimeout`](waitForTimeout.md) or [`waitForSelector`](waitForSelector.md) when an agent caller needs an action-specific output line such as `WAIT_TIMEOUT` or `SELECTOR`.

## Arguments

- `<target>`: Delay in milliseconds or CSS selector/supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum selector wait time. Ignored when `<target>` is numeric. The scripting default is `5000` when omitted.

## Stdout

Numeric delay:

```text
PASS 001 wait 25
```

Selector wait:

```text
PASS 001 wait #ready
```

## Stderr

Writes invalid-duration, browser, selector, timeout, missing-element, or action errors.

## Exit Codes

- `0`: Delay completed, or selector matched before the timeout.
- `1`: Delay was invalid, browser is not running, no element matched, or the action failed.

## Examples

```powershell
cmg browser control wait auto 250
cmg browser control wait auto "#ready" --timeout 5000
```

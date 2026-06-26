# `browser control wait selector`

Runs the scripting `waitForSelector` action once from the command line.

```powershell
cmg browser control wait selector "<selector>" [--state <state>] [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.
- `--state <state>`: Optional selector state. Supports `attached`, `detached`, `visible`, and `hidden`. Defaults to `attached`.

## Stdout

```text
PASS 001 waitForSelector #ready
SELECTOR 001 #ready
PASS 001 waitForSelector #ready
SELECTOR 001 #ready state=visible
```

## Stderr

Writes browser, selector, timeout, missing-element, or invalid-state errors. State timeouts include the last observed attached/visible state.

## Exit Codes

- `0`: Selector reached the requested state before the timeout.
- `1`: Browser is not running, the selector did not reach the requested state, no element matched, or the action failed.

## Example

```powershell
cmg browser control wait selector "text=Saved" --timeout 5000
cmg browser control wait selector "#toast" --state hidden --timeout 10000
```

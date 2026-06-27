# `browser control assertions waitForHidden`

Runs the scripting `waitForHidden` action once from the command line.

```powershell
cmg browser control assertions waitForHidden "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 waitForHidden #toast
EXPECT 001 hidden #toast
```

## Stderr

Writes browser, selector, timeout, hidden-state, or action errors.

## Exit Codes

- `0`: Element became hidden, detached, or missing before the timeout.
- `1`: Browser is not running, the element stayed visible, or the action failed.

## Example

```powershell
cmg browser control assertions waitForHidden "#toast" --timeout 5000
```

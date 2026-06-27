# `browser control assertions toBeHidden`

Runs the scripting `toBeHidden` action once from the command line.

```powershell
cmg browser control assertions toBeHidden "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectHidden`](expectHidden.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 toBeHidden #toast
EXPECT 001 hidden #toast
```

## Stderr

Writes browser, selector, timeout, hidden-state, or action errors.

## Exit Codes

- `0`: Element became hidden, detached, or missing before the timeout.
- `1`: Browser is not running, the element stayed visible, or the action failed.

## Example

```powershell
cmg browser control assertions toBeHidden "#toast"
```

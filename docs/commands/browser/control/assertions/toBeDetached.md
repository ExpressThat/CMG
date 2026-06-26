# `browser control assertions toBeDetached`

Runs the scripting `toBeDetached` action once from the command line.

```powershell
cmg browser control assertions toBeDetached "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectDetached`](expectDetached.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeDetached #toast
EXPECT 001 detached #toast
```

## Stderr

Writes browser, selector, timeout, detached-state, or action errors.

## Exit Codes

- `0`: No matching connected element existed.
- `1`: Browser is not running, the element was still attached, or the action failed.

## Example

```powershell
cmg browser control assertions toBeDetached "#toast"
```

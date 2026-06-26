# `browser control assertions toBeDisabled`

Runs the scripting `toBeDisabled` action once from the command line.

```powershell
cmg browser control assertions toBeDisabled "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectDisabled`](expectDisabled.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 toBeDisabled #save
EXPECT 001 disabled #save
```

## Stderr

Writes browser, selector, timeout, disabled-state, or action errors.

## Exit Codes

- `0`: Element became disabled before the timeout.
- `1`: Browser is not running, no element matched, element was enabled, or the action failed.

## Example

```powershell
cmg browser control assertions toBeDisabled "#save"
```

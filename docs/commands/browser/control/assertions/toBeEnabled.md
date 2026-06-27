# `browser control assertions toBeEnabled`

Runs the scripting `toBeEnabled` action once from the command line.

```powershell
cmg browser control assertions toBeEnabled "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectEnabled`](expectEnabled.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 toBeEnabled #save
EXPECT 001 enabled #save
```

## Stderr

Writes browser, selector, timeout, enabled-state, or action errors.

## Exit Codes

- `0`: Element became enabled before the timeout.
- `1`: Browser is not running, no element matched, element was disabled, or the action failed.

## Example

```powershell
cmg browser control assertions toBeEnabled "#save"
```

# `browser control assertions toHaveAccessibleName`

Runs the scripting `toHaveAccessibleName` action once from the command line.

```powershell
cmg browser control assertions toHaveAccessibleName "<selector>" "<expected>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectAccessibleName`](expectAccessibleName.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected accessible name fragment.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toHaveAccessibleName #save Save
EXPECT 001 accessiblename #save
```

## Stderr

Writes browser, selector, timeout, accessible-name mismatch, or action errors.

## Exit Codes

- `0`: Accessible name contained the expected text before the timeout.
- `1`: Browser is not running, no element matched, accessible name did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveAccessibleName "#save" "Save"
```

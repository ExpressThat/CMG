# `browser control assertions toBeNotFocused`

Playwright-style alias for [`expectNotFocused`](expectNotFocused.md).

```powershell
cmg browser control assertions toBeNotFocused "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotFocused #save
EXPECT 001 notfocused #save
```

## Stderr

Writes browser, selector, timeout, focus-state mismatch, or action errors.

## Exit Codes

- `0`: Element was not focused.
- `1`: Browser is not running, no element matched, element stayed focused, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotFocused "#save"
```

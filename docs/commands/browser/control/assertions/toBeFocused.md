# `browser control assertions toBeFocused`

Runs the scripting `toBeFocused` action once from the command line.

```powershell
cmg browser control assertions toBeFocused "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectFocused`](expectFocused.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeFocused #name
EXPECT 001 focused #name
```

## Stderr

Writes browser, selector, timeout, focus-state, or action errors.

## Exit Codes

- `0`: Element was focused.
- `1`: Browser is not running, no element matched, element was not focused, or the action failed.

## Example

```powershell
cmg browser control assertions toBeFocused "#name"
```

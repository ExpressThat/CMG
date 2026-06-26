# `browser control assertions toBeEditable`

Runs the scripting `toBeEditable` action once from the command line.

```powershell
cmg browser control assertions toBeEditable "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectEditable`](expectEditable.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeEditable #name
EXPECT 001 editable #name
```

## Stderr

Writes browser, selector, timeout, editable-state, or action errors.

## Exit Codes

- `0`: Element was editable.
- `1`: Browser is not running, no element matched, element was not editable, or the action failed.

## Example

```powershell
cmg browser control assertions toBeEditable "#name"
```

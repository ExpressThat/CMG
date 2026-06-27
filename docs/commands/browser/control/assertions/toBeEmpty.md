# `browser control assertions toBeEmpty`

Runs the scripting `toBeEmpty` action once from the command line.

```powershell
cmg browser control assertions toBeEmpty "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectEmpty`](expectEmpty.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeEmpty #name
EXPECT 001 empty #name
```

## Stderr

Writes browser, selector, timeout, empty-state, or action errors.

## Exit Codes

- `0`: Element text/value was empty.
- `1`: Browser is not running, no element matched, element was not empty, or the action failed.

## Example

```powershell
cmg browser control assertions toBeEmpty "#name"
```

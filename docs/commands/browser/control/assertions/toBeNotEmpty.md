# `browser control assertions toBeNotEmpty`

Playwright-style alias for [`expectNotEmpty`](expectNotEmpty.md).

```powershell
cmg browser control assertions toBeNotEmpty "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotEmpty #status
EXPECT 001 notempty #status
```

## Stderr

Writes browser, selector, timeout, empty-state mismatch, or action errors.

## Exit Codes

- `0`: Element text or value was not empty.
- `1`: Browser is not running, no element matched, element stayed empty, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotEmpty "#status"
```

# `browser control assertions toBeNotAttached`

Playwright-style alias for [`expectNotAttached`](expectNotAttached.md).

```powershell
cmg browser control assertions toBeNotAttached "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotAttached #toast
EXPECT 001 detached #toast
```

## Stderr

Writes browser, selector, timeout, attachment-state mismatch, or action errors.

## Exit Codes

- `0`: Element was detached or missing.
- `1`: Browser is not running, element stayed attached, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotAttached "#toast"
```

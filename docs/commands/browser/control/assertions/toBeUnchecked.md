# `browser control assertions toBeUnchecked`

Playwright-style alias for [`expectUnchecked`](expectUnchecked.md).

```powershell
cmg browser control assertions toBeUnchecked "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeUnchecked #marketing
EXPECT 001 unchecked #marketing
```

## Stderr

Writes browser, selector, timeout, checked-state mismatch, or action errors.

## Exit Codes

- `0`: Element checked state was false.
- `1`: Browser is not running, no element matched, element stayed checked, or the action failed.

## Example

```powershell
cmg browser control assertions toBeUnchecked "#marketing"
```

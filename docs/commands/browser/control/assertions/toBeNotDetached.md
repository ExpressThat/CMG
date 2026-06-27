# `browser control assertions toBeNotDetached`

Playwright-style alias for [`expectNotDetached`](expectNotDetached.md).

```powershell
cmg browser control assertions toBeNotDetached "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotDetached #save
EXPECT 001 attached #save
```

## Stderr

Writes browser, selector, timeout, attachment-state mismatch, or action errors.

## Exit Codes

- `0`: Element was attached.
- `1`: Browser is not running, no element matched, element stayed detached, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotDetached "#save"
```

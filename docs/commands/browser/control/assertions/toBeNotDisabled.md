# `browser control assertions toBeNotDisabled`

Playwright-style alias for [`expectNotDisabled`](expectNotDisabled.md).

```powershell
cmg browser control assertions toBeNotDisabled "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotDisabled #save
EXPECT 001 enabled #save
```

## Stderr

Writes browser, selector, timeout, disabled-state mismatch, or action errors.

## Exit Codes

- `0`: Element was enabled.
- `1`: Browser is not running, no element matched, element stayed disabled, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotDisabled "#save"
```

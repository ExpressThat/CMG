# `browser control assertions toBeNotEnabled`

Playwright-style alias for [`expectNotEnabled`](expectNotEnabled.md).

```powershell
cmg browser control assertions toBeNotEnabled "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotEnabled #archive
EXPECT 001 disabled #archive
```

## Stderr

Writes browser, selector, timeout, enabled-state mismatch, or action errors.

## Exit Codes

- `0`: Element was disabled or aria-disabled.
- `1`: Browser is not running, no element matched, element stayed enabled, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotEnabled "#archive"
```

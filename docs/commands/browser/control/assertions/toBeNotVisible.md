# `browser control assertions toBeNotVisible`

Playwright-style alias for [`expectNotVisible`](expectNotVisible.md).

```powershell
cmg browser control assertions toBeNotVisible "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeNotVisible #spinner
EXPECT 001 hidden #spinner
```

## Stderr

Writes browser, selector, timeout, visibility, or action errors.

## Exit Codes

- `0`: Element was hidden, detached, missing, or not visible.
- `1`: Browser is not running, element stayed visible, or the action failed.

## Example

```powershell
cmg browser control assertions toBeNotVisible "#spinner" --timeout 5000
```

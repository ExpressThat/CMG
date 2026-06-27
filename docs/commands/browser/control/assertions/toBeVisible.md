# `browser control assertions toBeVisible`

Runs the scripting `toBeVisible` action once from the command line.

```powershell
cmg browser control assertions toBeVisible "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectVisible`](expectVisible.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 toBeVisible #save
EXPECT 001 visible #save
```

## Stderr

Writes browser, selector, timeout, visibility, or action errors.

## Exit Codes

- `0`: Element became visible before the timeout.
- `1`: Browser is not running, no element matched, element was not visible, or the action failed.

## Example

```powershell
cmg browser control assertions toBeVisible "#save"
```

# `browser control assertions toHaveRole`

Runs the scripting `toHaveRole` action once from the command line.

```powershell
cmg browser control assertions toHaveRole "<selector>" "<expected>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectRole`](expectRole.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected explicit or implicit role.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toHaveRole #save button
EXPECT 001 role #save
```

## Stderr

Writes browser, selector, timeout, role mismatch, or action errors.

## Exit Codes

- `0`: Element role matched before the timeout.
- `1`: Browser is not running, no element matched, role did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveRole "#save" "button"
```

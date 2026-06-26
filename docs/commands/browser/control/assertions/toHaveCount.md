# `browser control assertions toHaveCount`

Runs the scripting `toHaveCount` action once from the command line.

```powershell
cmg browser control assertions toHaveCount "<selector>" <expected> [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectCount`](expectCount.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected matching element count.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 toHaveCount .row 2
EXPECT 001 count .row
```

## Stderr

Writes browser, selector, timeout, count mismatch, or action errors.

## Exit Codes

- `0`: Matching element count reached the expected value before the timeout.
- `1`: Browser is not running, count did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveCount ".row" 2
```

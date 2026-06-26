# `browser control assertions toHaveValues`

Runs the scripting `toHaveValues` action once from the command line.

```powershell
cmg browser control assertions toHaveValues "<selector>" "<expected>"... [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectValues`](expectValues.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: One or more selected values expected in order.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toHaveValues #plans basic pro
EXPECT 001 values #plans
```

## Stderr

Writes browser, selector, timeout, selected-value mismatch, or action errors.

## Exit Codes

- `0`: Selected values matched before the timeout.
- `1`: Browser is not running, no element matched, selected values did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveValues "#plans" "basic" "pro"
```

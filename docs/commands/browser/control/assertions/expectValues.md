# `browser control assertions expectValues`

Runs the scripting `expectValues` action once from the command line.

```powershell
cmg browser control assertions expectValues "<selector>" "<expected>"... [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: One or more selected values expected in order.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectValues #plans basic pro
EXPECT 001 values #plans
```

## Stderr

Writes browser, selector, timeout, selected-value mismatch, or action errors.

## Exit Codes

- `0`: Selected values matched before the timeout.
- `1`: Browser is not running, no element matched, selected values did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectValues "#plans" "basic" "pro"
```

# `browser control assertions expectCount`

Runs the scripting `expectCount` action once from the command line.

```powershell
cmg browser control assertions expectCount "<selector>" <expected> [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected matching element count.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 expectCount .row 2
EXPECT 001 count .row
```

## Stderr

Writes browser, selector, timeout, count mismatch, or action errors.

## Exit Codes

- `0`: Matching element count reached the expected value before the timeout.
- `1`: Browser is not running, count did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectCount ".row" 2
```

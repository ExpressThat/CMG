# `browser control assertions expectValue`

Runs the scripting `expectValue` action once from the command line.

```powershell
cmg browser control assertions expectValue "<selector>" "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected value fragment.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 expectValue #name CMG
EXPECT 001 value #name
```

## Stderr

Writes browser, selector, timeout, value mismatch, or action errors.

## Exit Codes

- `0`: Element value contained the expected text before the timeout.
- `1`: Browser is not running, no element matched, value did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectValue "#name" "CMG"
```

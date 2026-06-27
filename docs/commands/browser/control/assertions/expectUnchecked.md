# `browser control assertions expectUnchecked`

Runs the scripting `expectUnchecked` action once from the command line.

```powershell
cmg browser control assertions expectUnchecked "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectUnchecked #marketing
EXPECT 001 unchecked #marketing
```

## Stderr

Writes browser, selector, timeout, checked-state mismatch, or action errors.

## Exit Codes

- `0`: Element checked state was false.
- `1`: Browser is not running, no element matched, element stayed checked, or the action failed.

## Example

```powershell
cmg browser control assertions expectUnchecked "#marketing"
```

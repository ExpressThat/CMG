# `browser control assertions expectNotFocused`

Runs the scripting `expectNotFocused` action once from the command line.

```powershell
cmg browser control assertions expectNotFocused "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotFocused #save
EXPECT 001 notfocused #save
```

## Stderr

Writes browser, selector, timeout, focus-state mismatch, or action errors.

## Exit Codes

- `0`: Element was not focused.
- `1`: Browser is not running, no element matched, element stayed focused, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotFocused "#save"
```

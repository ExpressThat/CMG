# `browser control assertions expectEditable`

Runs the scripting `expectEditable` action once from the command line.

```powershell
cmg browser control assertions expectEditable "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectEditable #name
EXPECT 001 editable #name
```

## Stderr

Writes browser, selector, timeout, editable-state, or action errors.

## Exit Codes

- `0`: Element was editable.
- `1`: Browser is not running, no element matched, element was not editable, or the action failed.

## Example

```powershell
cmg browser control assertions expectEditable "#name"
```

# `browser control assertions expectNotEditable`

Runs the scripting `expectNotEditable` action once from the command line.

```powershell
cmg browser control assertions expectNotEditable "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotEditable #readonly
EXPECT 001 noteditable #readonly
```

## Stderr

Writes browser, selector, timeout, editability mismatch, or action errors.

## Exit Codes

- `0`: Element was not editable.
- `1`: Browser is not running, no element matched, element stayed editable, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotEditable "#readonly"
```

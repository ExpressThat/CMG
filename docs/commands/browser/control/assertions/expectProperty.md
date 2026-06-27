# `browser control assertions expectProperty`

Runs the scripting `expectProperty` action once from the command line.

```powershell
cmg browser control assertions expectProperty "<selector>" "<property>" "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<property>`: DOM property path, such as `value`, `checked`, or `dataset.ready`.
- `<expected>`: Expected string value or value fragment.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectProperty #save dataset.ready true
EXPECT 001 property #save
```

## Stderr

Writes browser, selector, timeout, property mismatch, or action errors.

## Exit Codes

- `0`: DOM property contained the expected value.
- `1`: Browser is not running, no element matched, property value did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectProperty "#save" "dataset.ready" "true"
```

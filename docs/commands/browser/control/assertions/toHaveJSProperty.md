# `browser control assertions toHaveJSProperty`

Runs the scripting `toHaveJSProperty` action once from the command line.

```powershell
cmg browser control assertions toHaveJSProperty "<selector>" "<property>" "<expected>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectProperty`](expectProperty.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<property>`: DOM property path, such as `value`, `checked`, or `dataset.ready`.
- `<expected>`: Expected string value or value fragment.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toHaveJSProperty #save dataset.ready true
EXPECT 001 property #save
```

## Stderr

Writes browser, selector, timeout, property mismatch, or action errors.

## Exit Codes

- `0`: DOM property contained the expected value.
- `1`: Browser is not running, no element matched, property value did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveJSProperty "#save" "dataset.ready" "true"
```

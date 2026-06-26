# `browser control assertions disabled`

Runs the scripting `expectDisabled` action once from the command line.

```powershell
cmg browser control assertions disabled "<selector>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <ms>`: Poll until the element is disabled or the timeout expires.

## Stdout

```text
PASS 001 expectDisabled #save
EXPECT 001 disabled #save
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element was disabled.
- `1`: Browser is not running, no element matched, element was enabled, or the timeout expired.

## Example

```powershell
cmg browser control assertions disabled "#save" --timeout 5000
```

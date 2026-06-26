# `browser control assertions enabled`

Runs the scripting `expectEnabled` action once from the command line.

```powershell
cmg browser control assertions enabled "<selector>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <ms>`: Poll until the element is enabled or the timeout expires.

## Stdout

```text
PASS 001 expectEnabled #save
EXPECT 001 enabled #save
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element was enabled.
- `1`: Browser is not running, no element matched, element was disabled, or the timeout expired.

## Example

```powershell
cmg browser control assertions enabled "#save"
```

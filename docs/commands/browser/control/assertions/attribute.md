# `browser control assertions attribute`

Runs the scripting `expectAttribute` action once from the command line.

```powershell
cmg browser control assertions attribute "<selector>" "<name>" "<expected>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<name>`: Attribute name.
- `<expected>`: Text fragment expected in the attribute value.

## Options

- `--timeout <ms>`: Poll until the attribute value contains the expected text or the timeout expires.

## Stdout

```text
PASS 001 expectAttribute #save data-state ready
EXPECT 001 attribute #save
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Attribute value contained the expected text.
- `1`: Browser is not running, no element matched, attribute did not match, or the timeout expired.

## Example

```powershell
cmg browser control assertions attribute "#save" "data-state" "ready"
```

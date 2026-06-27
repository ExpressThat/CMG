# `browser control assertions value`

Runs the scripting `expectValue` action once from the command line.

```powershell
cmg browser control assertions value "<selector>" "<expected>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Text fragment expected in the element value.

## Options

- `--timeout <ms>`: Poll until the element value contains the expected text or the timeout expires.

## Stdout

```text
PASS 001 expectValue #name CMG
EXPECT 001 value #name
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element value contained the expected text.
- `1`: Browser is not running, no element matched, value did not match, or the timeout expired.

## Example

```powershell
cmg browser control assertions value "#name" "CMG"
```

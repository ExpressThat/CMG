# `browser control assertions count`

Runs the scripting `expectCount` action once from the command line.

```powershell
cmg browser control assertions count "<selector>" <expected> [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected number of matching elements.

## Options

- `--timeout <ms>`: Poll until the count matches or the timeout expires.

## Stdout

```text
PASS 001 expectCount .row 2
EXPECT 001 count .row
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Matching element count equaled `<expected>`.
- `1`: Browser is not running, count did not match, or the timeout expired.

## Example

```powershell
cmg browser control assertions count ".row" 2
```

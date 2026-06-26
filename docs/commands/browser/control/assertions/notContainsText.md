# `browser control assertions notContainsText`

Runs the scripting `notContainsText` action once from the command line.

```powershell
cmg browser control assertions notContainsText "<selector>" "<expected>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Text fragment that must not appear in the element's visible text.

## Options

- `--timeout <ms>`: Poll until the text is absent or the timeout expires.

## Stdout

```text
PASS 001 notContainsText #status Error
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element text did not contain the expected text.
- `1`: Browser is not running, no element matched, assertion failed, or the timeout expired.

## Example

```powershell
cmg browser control assertions notContainsText "#status" "Error"
```

# `browser control assertions expectNoText`

Runs the scripting `expectNoText` action once from the command line.

```powershell
cmg browser control assertions expectNoText "<selector>" "<expected>" [--timeout <ms>] [--match <mode>] [--ignore-case]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Text fragment that must not appear in the element's visible text.

## Options

- `--timeout <ms>`: Poll until the text is absent or the timeout expires.
- `--match <mode>`: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Match text case-insensitively.

## Stdout

```text
PASS 001 expectNoText #status Error
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element text did not contain the expected text.
- `1`: Browser is not running, no element matched, assertion failed, or the timeout expired.

## Example

```powershell
cmg browser control assertions expectNoText "#status" "Error" --timeout 5000
```

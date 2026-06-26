# `browser control assertions notContains`

Runs the scripting `notContains` action once from the command line.

```powershell
cmg browser control assertions notContains "<expected>" [--timeout <ms>] [--match <mode>] [--ignore-case]
```

## Arguments

- `<expected>`: Text fragment that must not appear in the page body's visible text.

## Options

- `--timeout <ms>`: Poll until the body text no longer contains the text or the timeout expires.
- `--match <mode>`: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Match text case-insensitively.

## Stdout

```text
PASS 001 notContains Error
```

## Stderr

Writes browser, body text, timeout, or assertion failure errors.

## Exit Codes

- `0`: Page body text did not contain the expected text.
- `1`: Browser is not running, body text still matched, or the timeout expired.

## Example

```powershell
cmg browser control assertions notContains "Error" --timeout 5000
```

# `browser control assertions contains`

Runs the scripting `contains` action once from the command line.

```powershell
cmg browser control assertions contains "<expected>" [--timeout <ms>]
```

## Arguments

- `<expected>`: Text fragment expected in the page body's visible text.

## Options

- `--timeout <ms>`: Poll until the body text contains the expected text or the timeout expires.

## Stdout

```text
PASS 001 contains Welcome
```

## Stderr

Writes browser, body text, timeout, or assertion failure errors.

## Exit Codes

- `0`: Page body text contained the expected text.
- `1`: Browser is not running, body text did not match, or the timeout expired.

## Example

```powershell
cmg browser control assertions contains "Welcome" --timeout 5000
```

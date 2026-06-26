# `browser control events pageErrors waitForPageError`

Runs the scripting `waitForPageError` action once from the command line.

```powershell
cmg browser control events pageErrors waitForPageError "<text>" [--timeout <milliseconds>]
```

## Arguments

- `<text>`: Page error text to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds.

## Stdout

```text
PASS 001 waitForPageError Boom
PAGE_ERROR 001 Boom
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching page error was observed.
- `1`: Browser is not running, arguments are invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events pageErrors waitForPageError "Boom" --timeout 5000
```

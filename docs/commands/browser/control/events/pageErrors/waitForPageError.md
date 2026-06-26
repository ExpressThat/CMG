# `browser control events pageErrors waitForPageError`

Runs the scripting `waitForPageError` action once from the command line.

```powershell
cmg browser control events pageErrors waitForPageError "<text>" [options]
```

## Arguments

- `<text>`: Page error text to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds.
- `--match <contains|exact|regex>`: Page-error text match mode. Default is `contains`.
- `--ignore-case`: Match page-error text without case sensitivity.

## Stdout

```text
PASS 001 waitForPageError Boom
PAGE_ERROR 001 Boom
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching page error was observed.
- `1`: Browser is not running, arguments are invalid, the regex is invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events pageErrors waitForPageError "Boom" --match contains --ignore-case --timeout 5000
```

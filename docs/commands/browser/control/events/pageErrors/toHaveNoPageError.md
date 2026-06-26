# `browser control events pageErrors toHaveNoPageError`

Provider-style alias for [`expectNoPageError`](expectNoPageError.md).

```powershell
cmg browser control events pageErrors toHaveNoPageError [text] [options]
```

## Arguments

- `[text]`: Optional page error substring to reject. If omitted, any captured page error fails.

## Options

- `--timeout <milliseconds>`: Observation window in milliseconds. Default is `0`.
- `--match <contains|exact|regex>`: Page-error text match mode when `[text]` is provided. Default is `contains`.
- `--ignore-case`: Match page-error text without case sensitivity.

## Stdout

```text
PASS 001 toHaveNoPageError Cannot read
PAGE_ERROR_OK 001
```

## Stderr

Writes browser, argument, option, parse, or action errors. A matching page error fails with the captured error text.

## Exit Codes

- `0`: No matching page error was captured during the observation window.
- `1`: Browser is not running, arguments or options are invalid, the regex is invalid, or a matching page error was captured.

## Examples

```powershell
cmg browser control events pageErrors capturePageErrors
cmg browser control events pageErrors toHaveNoPageError "Cannot read" --match exact --timeout 500
```

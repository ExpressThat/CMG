# `browser control events console toHaveNoConsole`

Provider-style alias for [`expectNoConsole`](expectNoConsole.md).

```powershell
cmg browser control events console toHaveNoConsole [text] [options]
```

## Arguments

- `[text]`: Optional console message substring to reject. If omitted, any matching-level console message fails.

## Options

- `--level <level>`: Console level filter: `log`, `info`, `warn`, or `error`. Default is `error`.
- `--timeout <milliseconds>`: Observation window in milliseconds. Default is `0`.
- `--match <contains|exact|regex>`: Text match mode when `[text]` is provided. Default is `contains`.
- `--ignore-case`: Match console text without case sensitivity.

## Stdout

```text
PASS 001 toHaveNoConsole deprecated
CONSOLE_OK 001 level=warn
```

## Stderr

Writes browser, argument, option, parse, or action errors. A matching console message fails with the message text.

## Exit Codes

- `0`: No matching console message was captured during the observation window.
- `1`: Browser is not running, arguments or options are invalid, the regex is invalid, or a matching console message was captured.

## Examples

```powershell
cmg browser control events console captureConsole
cmg browser control events console toHaveNoConsole "deprecated" --match exact --level warn --timeout 500
```

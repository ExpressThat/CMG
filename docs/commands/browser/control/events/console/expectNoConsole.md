# `browser control events console expectNoConsole`

Runs the scripting `expectNoConsole` action once from the command line.

```powershell
cmg browser control events console expectNoConsole [text] [--level <level>] [--timeout <milliseconds>]
```

## Arguments

- `[text]`: Optional console message substring to reject. If omitted, any matching-level console message fails.

## Options

- `--level <level>`: Console level filter: `log`, `info`, `warn`, or `error`. Default is `error`.
- `--timeout <milliseconds>`: Observation window in milliseconds. Default is `0`.

## Stdout

```text
PASS 001 expectNoConsole
CONSOLE_OK 001 level=error
```

## Stderr

Writes browser, argument, option, parse, or action errors. A matching console message fails with the message text.

## Exit Codes

- `0`: No matching console message was captured during the observation window.
- `1`: Browser is not running, arguments or options are invalid, or a matching console message was captured.

## Examples

```powershell
cmg browser control events console captureConsole
cmg browser control events console expectNoConsole --level error --timeout 250
cmg browser control events console expectNoConsole "deprecated" --level warn
```

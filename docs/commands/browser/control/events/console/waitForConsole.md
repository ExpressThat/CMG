# `browser control events console waitForConsole`

Runs the scripting `waitForConsole` action once from the command line.

```powershell
cmg browser control events console waitForConsole "<text>" [--level <level>] [--timeout <milliseconds>]
```

## Arguments

- `<text>`: Console message text to match.

## Options

- `--level <level>`: Console level filter: `log`, `info`, `warn`, or `error`.
- `--timeout <milliseconds>`: Timeout in milliseconds.

## Stdout

```text
PASS 001 waitForConsole Ready
CONSOLE 001 log Ready
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching console message was observed.
- `1`: Browser is not running, arguments are invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events console waitForConsole "Ready" --level log --timeout 5000
```

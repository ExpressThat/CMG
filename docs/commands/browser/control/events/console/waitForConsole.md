# `browser control events console waitForConsole`

Runs the scripting `waitForConsole` action once from the command line.

```powershell
cmg browser control events console waitForConsole "<text>" [options]
```

## Arguments

- `<text>`: Console message text to match.

## Options

- `--level <level>`: Console level filter: `log`, `info`, `warn`, or `error`.
- `--timeout <milliseconds>`: Timeout in milliseconds.
- `--match <contains|exact|regex>`: Text match mode. Default is `contains`.
- `--ignore-case`: Match console text without case sensitivity.

## Stdout

```text
PASS 001 waitForConsole Ready
CONSOLE 001 log Ready
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching console message was observed.
- `1`: Browser is not running, arguments are invalid, the regex is invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events console waitForConsole "^Ready$" --match regex --ignore-case --level log --timeout 5000
```

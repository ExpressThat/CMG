# `browser control events dialogs waitForDialog`

Runs the scripting `waitForDialog` action once from the command line.

```powershell
cmg browser control events dialogs waitForDialog "<text>" [options]
```

## Arguments

- `<text>`: Dialog message text to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds.
- `--match <contains|exact|regex>`: Dialog message match mode. Default is `contains`.
- `--ignore-case`: Match dialog text without case sensitivity.

## Stdout

```text
PASS 001 waitForDialog Confirm
DIALOG 001 confirm Confirm
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching dialog was observed.
- `1`: Browser is not running, arguments are invalid, the regex is invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events dialogs waitForDialog "^Confirm$" --match regex --timeout 5000
```

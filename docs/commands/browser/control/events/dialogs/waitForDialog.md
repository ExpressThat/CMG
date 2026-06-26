# `browser control events dialogs waitForDialog`

Runs the scripting `waitForDialog` action once from the command line.

```powershell
cmg browser control events dialogs waitForDialog "<text>" [--timeout <milliseconds>]
```

## Arguments

- `<text>`: Dialog message text to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds.

## Stdout

```text
PASS 001 waitForDialog Confirm
DIALOG 001 confirm Confirm
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching dialog was observed.
- `1`: Browser is not running, arguments are invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events dialogs waitForDialog "Confirm" --timeout 5000
```

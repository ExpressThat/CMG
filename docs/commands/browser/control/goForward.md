# `browser control goForward`

Runs the scripting `goForward` action once from the command line.

```powershell
cmg browser control goForward [--timeout <milliseconds>]
```

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 goForward
FORWARD 001 https://example.com/next
```

## Stderr

Writes browser, timeout, or navigation errors.

## Exit Codes

- `0`: History navigation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control goForward
```

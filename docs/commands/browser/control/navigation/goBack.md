# `browser control navigation goBack`

Runs the scripting `goBack` action once from the command line.

```powershell
cmg browser control navigation goBack [--timeout <milliseconds>]
```

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 goBack
BACK 001 https://example.com/previous
```

## Stderr

Writes browser, timeout, or navigation errors.

## Exit Codes

- `0`: History navigation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation goBack --timeout 10000
```

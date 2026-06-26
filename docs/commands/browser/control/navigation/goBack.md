# `browser control navigation goBack`

Runs the scripting `goBack` action once from the command line.

```powershell
cmg browser control navigation goBack [--timeout <milliseconds>] [--wait-until <state>]
```

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.
- `--wait-until <state>`: Optional post-navigation state to wait for. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.

## Stdout

```text
PASS 001 goBack
BACK 001 https://example.com/previous
```

With `--wait-until`, stdout includes the requested wait and final document state:

```text
PASS 001 goBack timeout=5000 waitUntil=domcontentloaded
BACK 001 https://example.com/previous waitUntil=domcontentloaded state=interactive
```

## Stderr

Writes browser, timeout, or navigation errors.

## Exit Codes

- `0`: History navigation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation goBack --timeout 10000
cmg browser control navigation goBack --wait-until networkidle --timeout 10000
```

# `browser control navigation goForward`

Runs the scripting `goForward` action once from the command line.

```powershell
cmg browser control navigation goForward [--timeout <milliseconds>] [--wait-until <state>]
```

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.
- `--wait-until <state>`: Optional post-navigation state to wait for. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.

## Stdout

```text
PASS 001 goForward
FORWARD 001 https://example.com/next
```

With `--wait-until`, stdout includes the requested wait and final document state:

```text
PASS 001 goForward timeout=5000 waitUntil=domcontentloaded
FORWARD 001 https://example.com/next waitUntil=domcontentloaded state=interactive
```

## Stderr

Writes browser, timeout, or navigation errors.

## Exit Codes

- `0`: History navigation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation goForward
cmg browser control navigation goForward --wait-until domcontentloaded --timeout 10000
```

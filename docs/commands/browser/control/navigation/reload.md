# `browser control navigation reload`

Runs the scripting `reload` action once from the command line.

```powershell
cmg browser control navigation reload [--wait-until <state>] [--timeout <milliseconds>]
```

## Options

- `--wait-until <state>`: Optional post-reload state to wait for. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.
- `--timeout <milliseconds>`: Maximum wait time when `--wait-until` waits for a page state. Default is `5000`.

## Stdout

```text
PASS 001 reload
RELOADED 001 https://example.com/
```

With `--wait-until`, stdout includes the requested wait and final document state:

```text
PASS 001 reload waitUntil=domcontentloaded timeout=5000
RELOADED 001 https://example.com/ waitUntil=domcontentloaded state=interactive
```

## Stderr

Writes browser, reload, timeout, or invalid state errors.

## Exit Codes

- `0`: Page reload was requested, and the requested post-reload state was reached when provided.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation reload
cmg browser control navigation reload --wait-until networkidle --timeout 10000
```

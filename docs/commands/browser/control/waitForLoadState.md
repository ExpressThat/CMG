# `browser control waitForLoadState`

Runs the scripting `waitForLoadState` action once from the command line.

```powershell
cmg browser control waitForLoadState [state] [--timeout <milliseconds>]
```

## Arguments

- `[state]`: `loading`, `interactive`, `complete`, `load`, or `networkidle`. Default is `load`.

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 waitForLoadState complete
LOAD_STATE 001 complete
```

## Stderr

Writes browser, timeout, or invalid state errors.

## Exit Codes

- `0`: The page reached the requested state.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control waitForLoadState complete --timeout 10000
```

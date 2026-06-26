# `browser control navigation waitForNetworkIdle`

Runs the scripting `waitForNetworkIdle` action once from the command line.

```powershell
cmg browser control navigation waitForNetworkIdle [--timeout <milliseconds>]
```

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 waitForNetworkIdle timeout=5000
NETWORK_IDLE 001 complete
```

## Stderr

Writes browser or timeout errors. Timeout errors include the requested timeout and last observed document state.

## Exit Codes

- `0`: The page reached network idle.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation waitForNetworkIdle --timeout 10000
```

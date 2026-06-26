# `browser control tabs wait`

Runs the scripting `waitForTab` action once from the command line.

```powershell
cmg browser control tabs wait --count <count> [--timeout <milliseconds>]
```

## Options

- `--count <count>`: Required minimum number of tabs to wait for.
- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 waitForTab
TAB_COUNT 001 2
```

## Stderr

Writes browser or timeout errors.

## Exit Codes

- `0`: At least the requested number of tabs existed before the timeout.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control tabs wait --count 2 --timeout 10000
```

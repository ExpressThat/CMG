# `browser control tabs waitForTab`

Runs the scripting `waitForTab` action once from the command line.

```powershell
cmg browser control tabs waitForTab --count <count> [--timeout <milliseconds>]
```

## Options

- `--count <count>`: Required minimum tab count to wait for.
- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 waitForTab count=2 timeout=5000
TAB_READY 001 count=2
```

## Stderr

Writes browser, timeout, parse, or action errors.

## Exit Codes

- `0`: The requested tab count was reached.
- `1`: Browser is not running or the wait timed out.

## Examples

```powershell
cmg browser control tabs waitForTab --count 2 --timeout 5000
```

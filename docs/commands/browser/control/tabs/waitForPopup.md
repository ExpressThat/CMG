# `browser control tabs waitForPopup`

Runs the scripting `waitForPopup` action once from the command line.

```powershell
cmg browser control tabs waitForPopup --count <count> [--timeout <milliseconds>]
```

## Options

- `--count <count>`: Required minimum tab or popup count to wait for.
- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 waitForPopup count=2 timeout=5000
TAB_READY 001 count=2
```

## Stderr

Writes browser, timeout, parse, or action errors.

## Exit Codes

- `0`: The requested tab or popup count was reached.
- `1`: Browser is not running or the wait timed out.

## Examples

```powershell
cmg browser control tabs waitForPopup --count 2 --timeout 5000
```

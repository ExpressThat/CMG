# `browser control navigation waitForUrl`

Runs the scripting `waitForUrl` action once from the command line.

```powershell
cmg browser control navigation waitForUrl "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<expected>`: URL substring to wait for.

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 waitForUrl checkout
URL 001 https://example.com/checkout
```

## Stderr

Writes browser, timeout, or URL mismatch errors.

## Exit Codes

- `0`: The current URL matched before the timeout.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation waitForUrl "checkout" --timeout 10000
```

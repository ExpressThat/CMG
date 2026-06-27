# `browser control navigation waitForUrl`

Runs the scripting `waitForUrl` action once from the command line.

```powershell
cmg browser control navigation waitForUrl "<expected>" [--timeout <milliseconds>] [--match <mode>] [--ignore-case]
```

## Arguments

- `<expected>`: URL text to wait for.

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.
- `--match <mode>`: Match mode: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Use case-insensitive matching.

## Stdout

```text
PASS 001 waitForUrl checkout
URL 001 https://example.com/checkout
```

## Stderr

Writes browser, timeout, option, regex, or URL mismatch errors. Timeout failures include the expected text, match mode, timeout, and last URL seen.

## Exit Codes

- `0`: The current URL matched before the timeout.
- `1`: Browser is not running, the URL did not match before the timeout, or the action failed.

## Example

```powershell
cmg browser control navigation waitForUrl "checkout/\\d+" --match regex --ignore-case --timeout 10000
```

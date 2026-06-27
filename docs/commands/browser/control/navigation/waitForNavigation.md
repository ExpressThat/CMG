# `browser control navigation waitForNavigation`

Runs the scripting `waitForNavigation` action once from the command line.

```powershell
cmg browser control navigation waitForNavigation [expected] [--wait-until <state>] [--timeout <milliseconds>]
```

## Arguments

- `[expected]`: Optional URL substring expected after navigation.

## Options

- `--wait-until <state>`: `load`, `domcontentloaded`, `networkidle`, or `commit`. Default is `load`.
- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 waitForNavigation checkout waitUntil=domcontentloaded
NAVIGATION 001 {"url":"https://example.com/checkout","state":"domcontentloaded"}
```

## Stderr

Writes browser, JavaScript, timeout, URL mismatch, or invalid-state errors.

## Exit Codes

- `0`: Navigation reached the requested state and optional URL match.
- `1`: Browser is not running, navigation did not match, the timeout expired, or the action failed.

## Examples

```powershell
cmg browser control navigation waitForNavigation "checkout" --wait-until domcontentloaded --timeout 10000
cmg browser control navigation waitForNavigation --wait-until networkidle
```

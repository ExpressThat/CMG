# `browser control wait element`

Runs the scripting `waitForElement` action once from the command line.

```powershell
cmg browser control wait element "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 waitForElement #ready
```

## Stderr

Writes browser, selector, timeout, or missing-element errors.

## Exit Codes

- `0`: Element existed before the timeout.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control wait element "#ready" --timeout 5000
```

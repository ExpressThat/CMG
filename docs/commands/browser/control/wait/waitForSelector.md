# `browser control wait waitForSelector`

Runs the scripting `waitForSelector` action once from the command line.

```powershell
cmg browser control wait waitForSelector "<selector>" [--timeout <milliseconds>]
```

This is an exact-name alias for [`selector`](selector.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 waitForSelector #ready
SELECTOR 001 #ready
```

## Stderr

Writes browser, selector, timeout, or missing-element errors.

## Exit Codes

- `0`: Selector matched before the timeout.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control wait waitForSelector "text=Saved" --timeout 5000
```

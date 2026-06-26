# `browser control wait selector`

Runs the scripting `waitForSelector` action once from the command line.

```powershell
cmg browser control wait selector "<selector>" [--timeout <milliseconds>]
```

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
cmg browser control wait selector "text=Saved" --timeout 5000
```

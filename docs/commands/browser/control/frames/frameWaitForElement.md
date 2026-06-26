# `browser control frames frameWaitForElement`

Runs the scripting `frameWaitForElement` action once from the command line.

```powershell
cmg browser control frames frameWaitForElement "<frameSelector>" "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector inside the iframe.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 frameWaitForElement #frame #email
FRAME 001 frameWaitForElement
```

## Stderr

Writes browser, frame, selector, timeout, parse, or action errors.

## Exit Codes

- `0`: Element appeared.
- `1`: Browser is not running, the frame is missing, or the wait timed out.

## Examples

```powershell
cmg browser control frames frameWaitForElement "#checkoutFrame" "#email" --timeout 5000
```

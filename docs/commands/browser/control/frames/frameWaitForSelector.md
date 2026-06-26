# `browser control frames frameWaitForSelector`

Waits for an element inside a same-origin iframe using the `frameWaitForSelector` script action.

```powershell
cmg browser control frames frameWaitForSelector "<frameSelector>" "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector inside the iframe.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 frameWaitForSelector #frame #ready timeout=5000
FRAME 001 frameWaitForSelector
```

## Stderr

Frame, selector, timeout, and browser errors are written to stderr with the action name and reason.

## Exit Codes

- `0`: Element was found.
- `1`: Browser is not running, the frame is missing, the selector is missing in the frame, or the action failed.

## Examples

```powershell
cmg browser control frames frameWaitForSelector "#checkoutFrame" "#ready" --timeout 5000
```

# `browser control frames frameCount`

Counts matching elements inside a same-origin iframe.

```powershell
cmg browser control frames frameCount "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.

## Stdout

```text
PASS 001 frameCount #frame .item
FRAME_COUNT 001 3
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Count was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameCount "#checkoutFrame" ".row"
```

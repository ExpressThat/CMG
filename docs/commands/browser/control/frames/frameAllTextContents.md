# `browser control frames frameAllTextContents`

Reads `textContent` for all matching elements inside a same-origin iframe.

```powershell
cmg browser control frames frameAllTextContents "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.

## Stdout

```text
PASS 001 frameAllTextContents #frame .item
FRAME_TEXTS 001 ["One","Two"]
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Text values were read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameAllTextContents "#checkoutFrame" ".item"
```

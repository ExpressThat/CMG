# `browser control frames frameBoundingBox`

Reads an element bounding box inside a same-origin iframe.

```powershell
cmg browser control frames frameBoundingBox "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.

## Stdout

```text
PASS 001 frameBoundingBox #frame #card
FRAME_BOUNDING_BOX 001 {"x":10,"y":20,"width":120,"height":40}
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Bounding box was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameBoundingBox "#checkoutFrame" "#card"
```

# `browser control frames frameTextContent`

Reads `textContent` from an element inside a same-origin iframe.

```powershell
cmg browser control frames frameTextContent "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.

## Stdout

```text
PASS 001 frameTextContent #frame #status
FRAME_TEXT 001 Ready
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Text was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameTextContent "#checkoutFrame" "#status"
```

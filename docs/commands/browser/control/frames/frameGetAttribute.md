# `browser control frames frameGetAttribute`

Reads an element attribute inside a same-origin iframe.

```powershell
cmg browser control frames frameGetAttribute "<frameSelector>" "<selector>" "<name>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.
- `<name>`: Attribute name.

## Stdout

```text
PASS 001 frameGetAttribute #frame #profile href
FRAME_ATTRIBUTE 001 /profile
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Attribute was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameGetAttribute "#checkoutFrame" "#profile" href
```

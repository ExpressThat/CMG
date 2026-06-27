# `browser control frames frameInputValue`

Reads an input-like element value inside a same-origin iframe.

```powershell
cmg browser control frames frameInputValue "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.

## Stdout

```text
PASS 001 frameInputValue #frame #email
FRAME_VALUE 001 agent@example.com
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Value was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameInputValue "#checkoutFrame" "#email"
```

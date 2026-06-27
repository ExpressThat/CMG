# `browser control frames frameComputedStyle`

Reads a computed CSS property from an element inside a same-origin iframe.

```powershell
cmg browser control frames frameComputedStyle "<frameSelector>" "<selector>" "<property>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.
- `<property>`: CSS property name, such as `display` or `background-color`.

## Stdout

```text
PASS 001 frameComputedStyle #frame #status display
FRAME_STYLE 001 block
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Computed style was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameComputedStyle "#checkoutFrame" "#status" display
```

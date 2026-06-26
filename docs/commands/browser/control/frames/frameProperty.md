# `browser control frames frameProperty`

Reads a JavaScript property from an element inside a same-origin iframe.

```powershell
cmg browser control frames frameProperty "<frameSelector>" "<selector>" "<name>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich locator inside the iframe.
- `<name>`: Dot-separated JavaScript property path, such as `dataset.state`.

## Stdout

```text
PASS 001 frameProperty #frame #status dataset.state
FRAME_PROPERTY 001 ready
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Property was read.
- `1`: Browser is not running, the frame is missing, or the action failed.

## Examples

```powershell
cmg browser control frames frameProperty "#checkoutFrame" "#status" dataset.state
```

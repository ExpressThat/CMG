# `browser control frames frameHover`

Runs the scripting `frameHover` action once from the command line.

```powershell
cmg browser control frames frameHover "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector inside the iframe.

## Stdout

```text
PASS 001 frameHover #frame #save
FRAME 001 frameHover
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Element was hovered.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control frames frameHover "#checkoutFrame" "#save"
```

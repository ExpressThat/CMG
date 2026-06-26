# `browser control frames frameFill`

Runs the scripting `frameFill` action once from the command line.

```powershell
cmg browser control frames frameFill "<frameSelector>" "<selector>" "<text>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector inside the iframe.
- `<text>`: Text to fill.

## Stdout

```text
PASS 001 frameFill #frame #email agent@example.com
FRAME 001 frameFill
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Element was filled.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control frames frameFill "#checkoutFrame" "#email" "agent@example.com"
```

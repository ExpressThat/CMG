# `browser control frames frameType`

Runs the scripting `frameType` action once from the command line.

```powershell
cmg browser control frames frameType "<frameSelector>" "<selector>" "<text>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich/provider locator inside the iframe.
- `<text>`: Text to type.

## Stdout

```text
PASS 001 frameType #frame #name CMG
FRAME 001 frameType
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Text was typed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control frames frameType "#checkoutFrame" "#email" "agent@example.com"
```
